namespace Meetlr.Application.Interfaces.Models;

/// <summary>
/// Request to create a calendar event via ICalendarService
/// </summary>
public record CalendarServiceEventRequest
{
    public Guid? BookingId { get; init; }
    public string Summary { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public string TimeZone { get; init; } = "UTC";
    public List<string> AttendeeEmails { get; init; } = new();
    public string? Location { get; init; }
    public string? MeetingLink { get; init; }
    public int BufferBeforeMinutes { get; init; }
    public int BufferAfterMinutes { get; init; }
}
