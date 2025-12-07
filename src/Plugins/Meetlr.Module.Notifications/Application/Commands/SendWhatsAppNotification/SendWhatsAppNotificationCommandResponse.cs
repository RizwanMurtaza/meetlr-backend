namespace Meetlr.Module.Notifications.Application.Commands.SendWhatsAppNotification;

public class SendWhatsAppNotificationCommandResponse
{
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
}
