namespace Meetlr.Module.Calendar.Application.Commands.CreateSeriesCalendarEvents;

public class BookingEventInfo
{
    public Guid BookingId { get; set; }
    public string InviteeName { get; set; } = string.Empty;
    public string InviteeEmail { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Location { get; set; }
    public string? MeetingLink { get; set; }
    public string? Notes { get; set; }
    public int OccurrenceNumber { get; set; }
}