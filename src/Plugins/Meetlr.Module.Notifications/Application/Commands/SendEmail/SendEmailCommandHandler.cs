using MediatR;
using Meetlr.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Meetlr.Module.Notifications.Application.Commands.SendEmail;

/// <summary>
/// Handler for sending emails through configured email providers
/// Uses IEmailService which handles provider failover logic
/// </summary>
public class SendEmailCommandHandler : IRequestHandler<SendEmailCommand, SendEmailCommandResponse>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<SendEmailCommandHandler> _logger;

    public SendEmailCommandHandler(
        IEmailService emailService,
        ILogger<SendEmailCommandHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<SendEmailCommandResponse> Handle(
        SendEmailCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Sending email to {To} with subject '{Subject}'",
                request.To,
                request.Subject);

            await _emailService.SendEmailAsync(
                request.To,
                request.Subject,
                request.Body,
                request.Attachments,
                cancellationToken);

            _logger.LogInformation(
                "Email sent successfully to {To}",
                request.To);

            return new SendEmailCommandResponse
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send email to {To} with subject '{Subject}'",
                request.To,
                request.Subject);

            return new SendEmailCommandResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
