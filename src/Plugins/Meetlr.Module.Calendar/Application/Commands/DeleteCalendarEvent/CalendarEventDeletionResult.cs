namespace Meetlr.Module.Calendar.Application.Commands.DeleteCalendarEvent;

public class CalendarEventDeletionResult
{
    public string Provider { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Error { get; set; }
}
