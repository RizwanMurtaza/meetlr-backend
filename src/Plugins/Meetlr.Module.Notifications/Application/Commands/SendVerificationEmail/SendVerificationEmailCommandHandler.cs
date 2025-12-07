using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Features.Emails.Commands.SendVerificationEmail;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Enums;
using Meetlr.Module.Notifications.Application.Commands.SendEmail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Module.Notifications.Application.Commands.SendVerificationEmail;

/// <summary>
/// Handler for sending email verification codes using well-designed templates
/// </summary>
public class SendVerificationEmailCommandHandler : IRequestHandler<SendVerificationEmailCommand, SendVerificationEmailCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly IEmailTemplateRenderer _emailTemplateRenderer;
    private readonly ILogger<SendVerificationEmailCommandHandler> _logger;

    public SendVerificationEmailCommandHandler(
        IUnitOfWork unitOfWork,
        IMediator mediator,
        IEmailTemplateRenderer emailTemplateRenderer,
        ILogger<SendVerificationEmailCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _emailTemplateRenderer = emailTemplateRenderer;
        _logger = logger;
    }

    public async Task<SendVerificationEmailCommandResponse> Handle(
        SendVerificationEmailCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Sending verification email to {Email} for user {UserId}",
                request.Email,
                request.UserId);

            // Get user and tenant info
            var user = await _unitOfWork.Repository<Meetlr.Domain.Entities.Users.User>().GetQueryable()
                .Include(u => u.Tenant)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for verification email", request.UserId);
                return new SendVerificationEmailCommandResponse
                {
                    Success = false,
                    ErrorMessage = "User not found"
                };
            }

            // Render template with variables
            var variables = new Dictionary<string, object>
            {
                ["userName"] = $"{user.FirstName} {user.LastName}",
                ["userFirstName"] = user.FirstName,
                ["otpCode"] = request.VerificationCode,
                ["tenantName"] = user.Tenant?.Name ?? "Calendly",
                ["expiryMinutes"] = "5"
            };

            var templateResult = await _emailTemplateRenderer.RenderAsync(
                EmailTemplateType.EmailVerification,
                variables,
                user.TenantId,
                null,
                cancellationToken);

            var (subject, htmlBody, _) = templateResult;

            // Send email using SendEmailCommand
            await _mediator.Send(new SendEmailCommand
            {
                To = request.Email,
                Subject = subject,
                Body = htmlBody
            }, cancellationToken);

            _logger.LogInformation(
                "Verification email sent successfully to {Email}",
                request.Email);

            return new SendVerificationEmailCommandResponse
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send verification email to {Email} for user {UserId}",
                request.Email,
                request.UserId);

            return new SendVerificationEmailCommandResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
