using MediatR;
using Meetlr.Application.Interfaces;

namespace Meetlr.Module.Notifications.Application.Commands.SendEmail;

/// <summary>
/// Command to send an email through the configured email providers
/// </summary>
public record SendEmailCommand : IRequest<SendEmailCommandResponse>
{
    public string To { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public List<EmailAttachment> Attachments { get; init; } = new();
}
