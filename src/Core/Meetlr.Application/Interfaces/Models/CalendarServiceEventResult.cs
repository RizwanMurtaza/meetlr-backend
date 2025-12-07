namespace Meetlr.Application.Interfaces.Models;

/// <summary>
/// Result from creating a calendar event via ICalendarService
/// </summary>
public record CalendarServiceEventResult
{
    public bool Success { get; init; }
    public string? EventId { get; init; }
    public string? Error { get; init; }

    public static CalendarServiceEventResult Succeeded(string eventId) =>
        new() { Success = true, EventId = eventId };

    public static CalendarServiceEventResult Failed(string error) =>
        new() { Success = false, Error = error };
}
