namespace Meetlr.Module.Calendar.Application.Commands.DeleteCalendarEvent;

public class DeleteCalendarEventResponse
{
    public bool Success { get; set; }
    public List<CalendarEventDeletionResult> Results { get; set; } = new();
}
