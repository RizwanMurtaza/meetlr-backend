using MediatR;

namespace Meetlr.Module.Notifications.Application.Commands.SendWhatsAppNotification;

public class SendWhatsAppNotificationCommand : IRequest<SendWhatsAppNotificationCommandResponse>
{
    public Guid NotificationPendingId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
