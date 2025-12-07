namespace Meetlr.Module.Notifications.Application.Commands.SendSmsNotification;

public class SendSmsNotificationCommandResponse
{
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
}
