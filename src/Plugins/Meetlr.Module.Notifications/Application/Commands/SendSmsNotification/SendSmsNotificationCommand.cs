using MediatR;

namespace Meetlr.Module.Notifications.Application.Commands.SendSmsNotification;

public class SendSmsNotificationCommand : IRequest<SendSmsNotificationCommandResponse>
{
    public Guid NotificationPendingId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string>? Metadata { get; set; }
}
