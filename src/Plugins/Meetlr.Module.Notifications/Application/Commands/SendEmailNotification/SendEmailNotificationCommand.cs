using MediatR;

namespace Meetlr.Module.Notifications.Application.Commands.SendEmailNotification;

public class SendEmailNotificationCommand : IRequest<SendEmailNotificationCommandResponse>
{
    public Guid NotificationPendingId { get; set; }
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? PlainTextBody { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
