namespace Meetlr.Module.Notifications.Application.Commands.SendEmailNotification;

public class SendEmailNotificationCommandResponse
{
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
}
