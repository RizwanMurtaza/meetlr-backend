namespace Meetlr.Application.Interfaces.Models;

/// <summary>
/// Represents a busy time slot from a calendar
/// </summary>
public record CalendarBusySlot
{
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public string? Source { get; init; }
}
