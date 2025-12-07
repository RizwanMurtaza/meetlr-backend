namespace Meetlr.Module.Calendar.Application.Commands.CreateCalendarEvent;

public class CalendarEventCreationResult
{
    public string Provider { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? EventId { get; set; }
    public string? Error { get; set; }
}
