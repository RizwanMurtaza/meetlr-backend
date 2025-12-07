using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Tenancy;
using Meetlr.Domain.Entities.Users;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.Http;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Authentication.Commands.VerifyEmail;

/// <summary>
/// Handler for email verification with OTP
/// Verifies OTP and marks email as confirmed
/// Returns JWT token upon successful verification
/// </summary>
public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, VerifyEmailCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOtpService _otpService;
    private readonly IIdentityService _identityService;

    public VerifyEmailCommandHandler(
        IUnitOfWork unitOfWork,
        IOtpService otpService,
        IIdentityService identityService)
    {
        _unitOfWork = unitOfWork;
        _otpService = otpService;
        _identityService = identityService;
    }

    public async Task<VerifyEmailCommandResponse> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
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

        // Check if already verified
        if (user.EmailConfirmed)
        {
            return new VerifyEmailCommandResponse
            {
                UserId = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                JwtToken = string.Empty,
                TokenExpiry = DateTime.UtcNow,
                Success = false,
                Message = "Email is already verified"
            };
        }

        // Validate OTP
        var isValid = await _otpService.ValidateOtpAsync(
            user.Id,
            request.OtpCode,
            OtpPurpose.EmailVerification,
            cancellationToken);

        if (!isValid)
        {
            return new VerifyEmailCommandResponse
            {
                UserId = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                JwtToken = string.Empty,
                TokenExpiry = DateTime.UtcNow,
                Success = false,
                Message = "Invalid or expired OTP code"
            };
        }

        // Mark email as confirmed
        user.EmailConfirmed = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var isAdmin = await _unitOfWork.Repository<UserGroup>()
            .GetQueryable()
            .AnyAsync(ug => ug.UserId == user.Id && ug.Group.IsAdminGroup, cancellationToken);
        // Generate JWT token
        var jwtToken = await _identityService.GenerateTokenAsync(user.Id, user.Email!,isAdmin, cancellationToken);
        var tokenExpiry = await _identityService.GetTokenExpiryAsync();

        return new VerifyEmailCommandResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            JwtToken = jwtToken,
            TokenExpiry = tokenExpiry,
            Success = true,
            Message = "Email verified successfully"
        };
    }
}
