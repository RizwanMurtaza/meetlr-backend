using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Features.Emails.Commands.SendPasswordResetEmail;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Enums;
using Meetlr.Module.Notifications.Application.Commands.SendEmail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Module.Notifications.Application.Commands.SendPasswordResetEmail;

/// <summary>
/// Handler for sending password reset emails using well-designed templates
/// </summary>
public class SendPasswordResetEmailCommandHandler : IRequestHandler<SendPasswordResetEmailCommand, SendPasswordResetEmailCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly IEmailTemplateRenderer _emailTemplateRenderer;
    private readonly ILogger<SendPasswordResetEmailCommandHandler> _logger;

    public SendPasswordResetEmailCommandHandler(
        IUnitOfWork unitOfWork,
        IMediator mediator,
        IEmailTemplateRenderer emailTemplateRenderer,
        ILogger<SendPasswordResetEmailCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _emailTemplateRenderer = emailTemplateRenderer;
        _logger = logger;
    }

    public async Task<SendPasswordResetEmailCommandResponse> Handle(
        SendPasswordResetEmailCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Sending password reset email to {Email} for user {UserId}",
                request.Email,
                request.UserId);

            // Get user and tenant info
            var user = await _unitOfWork.Repository<Meetlr.Domain.Entities.Users.User>().GetQueryable()
                .Include(u => u.Tenant)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for password reset email", request.UserId);
                return new SendPasswordResetEmailCommandResponse
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
                ["resetToken"] = request.ResetToken,
                ["tenantName"] = user.Tenant?.Name ?? "Calendly",
                ["expiryMinutes"] = "60"
            };

            var templateResult = await _emailTemplateRenderer.RenderAsync(
                EmailTemplateType.PasswordReset,
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
                "Password reset email sent successfully to {Email}",
                request.Email);

            return new SendPasswordResetEmailCommandResponse
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send password reset email to {Email} for user {UserId}",
                request.Email,
                request.UserId);

            return new SendPasswordResetEmailCommandResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
