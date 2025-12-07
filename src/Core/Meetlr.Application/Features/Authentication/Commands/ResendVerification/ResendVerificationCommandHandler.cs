using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Features.Emails.Commands.SendVerificationEmail;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Users;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Authentication.Commands.ResendVerification;

/// <summary>
/// Handler for resending verification email
/// Generates new OTP and sends verification email
/// </summary>
public class ResendVerificationCommandHandler : IRequestHandler<ResendVerificationCommand, ResendVerificationCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOtpService _otpService;
    private readonly IMediator _mediator;

    public ResendVerificationCommandHandler(
        IUnitOfWork unitOfWork,
        IOtpService otpService,
        IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _otpService = otpService;
        _mediator = mediator;
    }

    public async Task<ResendVerificationCommandResponse> Handle(ResendVerificationCommand request, CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _unitOfWork.Repository<User>()
            .GetQueryable()
            .Include(u => u.Tenant)
            .Where(u => u.Email == request.Email && !u.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            throw UserErrors.UserNotFoundByEmail(request.Email);
        }

        // Check if already verified
        if (user.EmailConfirmed)
        {
            return new ResendVerificationCommandResponse
            {
                Success = false,
                Message = "Email is already verified"
            };
        }

        // Rate limiting: Check if user recently requested an OTP (within 2 minutes)
        var recentOtp = await _unitOfWork.Repository<Meetlr.Domain.Entities.Users.OtpVerification>()
            .GetQueryable()
            .Where(o => o.UserId == user.Id &&
                        o.Purpose == OtpPurpose.EmailVerification &&
                        !o.IsDeleted &&
                        o.CreatedAt > DateTime.UtcNow.AddMinutes(-2))
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (recentOtp != null)
        {
            var secondsRemaining = (int)(120 - (DateTime.UtcNow - recentOtp.CreatedAt).TotalSeconds);
            throw AuthenticationErrors.TooManyRequests(secondsRemaining);
        }

        // Generate new OTP (invalidates any existing OTPs)
        var otpCode = await _otpService.GenerateOtpAsync(
            user.Id,
            OtpPurpose.EmailVerification,
            cancellationToken);

        // Send verification email
        try
        {
            // Send verification email using CQRS command
            await _mediator.Send(new SendVerificationEmailCommand
            {
                UserId = user.Id,
                Email = user.Email!,
                VerificationCode = otpCode
            }, cancellationToken);

            return new ResendVerificationCommandResponse
            {
                Success = true,
                Message = "Verification email sent successfully"
            };
        }
        catch (Exception ex)
        {
            return new ResendVerificationCommandResponse
            {
                Success = false,
                Message = $"Failed to send verification email: {ex.Message}"
            };
        }
    }
}
