namespace Meetlr.Module.Calendar.Application.Commands.CreateSeriesCalendarEvents;

public class CreateSeriesCalendarEventsCommandResponse
{
    public bool Success { get; set; }
    public int TotalCreated { get; set; }
    public int TotalFailed { get; set; }
    public Dictionary<Guid, string> CalendarEventIds { get; set; } = new(); // BookingId -> CalendarEventId JSON
    public string? ErrorMessage { get; set; }
}
