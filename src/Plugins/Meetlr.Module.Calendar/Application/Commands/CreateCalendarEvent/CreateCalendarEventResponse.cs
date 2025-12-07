namespace Meetlr.Module.Calendar.Application.Commands.CreateCalendarEvent;

public class CreateCalendarEventResponse
{
    public List<CalendarEventCreationResult> Results { get; set; } = new();
    public bool Success => Results.All(r => r.Success);
}
