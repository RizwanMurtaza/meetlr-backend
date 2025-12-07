namespace Meetlr.Application.Features.Notifications.Models;

/// <summary>
/// Payload structure for notifications stored in JSON
/// </summary>
public class NotificationPayload
{
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string? HtmlBody { get; set; }
    public string? MediaUrl { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
    public string? BookingId { get; set; }
    public string? MeetlrEventName { get; set; }
    public string? InviteeName { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    // For reschedule notifications - the old times before reschedule
    public DateTime? OldStartTime { get; set; }
    public DateTime? OldEndTime { get; set; }
}
