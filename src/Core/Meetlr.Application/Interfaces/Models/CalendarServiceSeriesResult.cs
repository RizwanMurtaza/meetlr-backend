namespace Meetlr.Application.Interfaces.Models;

/// <summary>
/// Result from creating a series of calendar events via ICalendarService
/// </summary>
public record CalendarServiceSeriesResult
{
    public bool Success { get; init; }
    public int SuccessCount { get; init; }
    public int FailureCount { get; init; }
    public List<CalendarServiceEventResult> Results { get; init; } = new();
}
