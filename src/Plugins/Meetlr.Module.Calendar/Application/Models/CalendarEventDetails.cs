namespace Meetlr.Module.Calendar.Application.Models;

/// <summary>
/// Details for creating/updating a calendar event
/// </summary>
public class CalendarEventDetails
{
    public string Summary { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public List<string> AttendeeEmails { get; set; } = new();
    public string? Location { get; set; }
    public string? MeetingLink { get; set; }
}
