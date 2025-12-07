using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Tenancy;
using Meetlr.Domain.Entities.Users;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Authentication.Commands.ResetPassword;

/// <summary>
/// Handler for resetting password
/// Validates OTP and updates user password
/// </summary>
public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOtpService _otpService;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IIdentityService _identityService;

    public ResetPasswordCommandHandler(
        IUnitOfWork unitOfWork,
        IOtpService otpService,
        IPasswordHasher<User> passwordHasher,
        IIdentityService identityService)
    {
        _unitOfWork = unitOfWork;
        _otpService = otpService;
        _passwordHasher = passwordHasher;
        _identityService = identityService;
    }

    public async Task<ResetPasswordCommandResponse> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _unitOfWork.Repository<User>()
            .GetQueryable()
            .Where(u => u.Email == request.Email && !u.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            throw NotFoundException.ForEntity("User", request.Email);
        }

        // Validate OTP
        var isValidOtp = await _otpService.ValidateOtpAsync(
            user.Id,
            request.OtpCode,
            OtpPurpose.PasswordReset,
            cancellationToken);

        if (!isValidOtp)
        {
            return new ResetPasswordCommandResponse
            {
                Success = false,
                Message = "Invalid or expired OTP code"
            };
        }

        // Hash new password
        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        // Update user
        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var isAdmin = await _unitOfWork.Repository<UserGroup>()
            .GetQueryable()
            .AnyAsync(ug => ug.UserId == user.Id && ug.Group.IsAdminGroup, cancellationToken);
        // OTP is already marked as used by ValidateOtpAsync

        
        // Optional: Generate token for auto-login
        var token = await _identityService.GenerateTokenAsync(user.Id, user.Email!,isAdmin, cancellationToken);

        return new ResetPasswordCommandResponse
        {
            Success = true,
            Message = "Password reset successfully",
            Token = token
        };
    }
}
