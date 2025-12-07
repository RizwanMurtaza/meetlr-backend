using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Features.Emails.Commands.SendPasswordResetEmail;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Users;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Authentication.Commands.ForgotPassword;

/// <summary>
/// Handler for initiating password reset
/// Generates OTP and sends password reset email
/// </summary>
public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOtpService _otpService;
    private readonly IMediator _mediator;

    public ForgotPasswordCommandHandler(
        IUnitOfWork unitOfWork,
        IOtpService otpService,
        IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _otpService = otpService;
        _mediator = mediator;
    }

    public async Task<ForgotPasswordCommandResponse> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _unitOfWork.Repository<User>()
            .GetQueryable()
            .Include(u => u.Tenant)
            .Where(u => u.Email == request.Email && !u.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        // Don't reveal whether user exists or not (security best practice)
        if (user == null)
        {
            return new ForgotPasswordCommandResponse
            {
                Success = true,
                Message = "If an account exists with this email, a password reset link has been sent"
            };
        }

        // Check if email is verified
        if (!user.EmailConfirmed)
        {
            return new ForgotPasswordCommandResponse
            {
                Success = false,
                Message = "Please verify your email before resetting password"
            };
        }

        // Rate limiting: Check if user recently requested password reset (within 2 minutes)
        var recentOtp = await _unitOfWork.Repository<OtpVerification>()
            .GetQueryable()
            .Where(o => o.UserId == user.Id &&
                        o.Purpose == OtpPurpose.PasswordReset &&
                        !o.IsDeleted &&
                        o.CreatedAt > DateTime.UtcNow.AddMinutes(-2))
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (recentOtp != null)
        {
            var secondsRemaining = (int)(120 - (DateTime.UtcNow - recentOtp.CreatedAt).TotalSeconds);
            throw AuthenticationErrors.TooManyRequests(secondsRemaining);
        }

        // Generate OTP for password reset (invalidates any existing password reset OTPs)
        var otpCode = await _otpService.GenerateOtpAsync(
            user.Id,
            OtpPurpose.PasswordReset,
            cancellationToken);

        // Send password reset email using CQRS command
        try
        {
            await _mediator.Send(new SendPasswordResetEmailCommand
            {
                UserId = user.Id,
                Email = user.Email!,
                ResetToken = otpCode
            }, cancellationToken);

            return new ForgotPasswordCommandResponse
            {
                Success = true,
                Message = "If an account exists with this email, a password reset link has been sent"
            };
        }
        catch (Exception ex)
        {
            return new ForgotPasswordCommandResponse
            {
                Success = false,
                Message = $"Failed to send password reset email: {ex.Message}"
            };
        }
    }
}
