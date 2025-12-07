namespace Meetlr.Application.Interfaces.Models;

/// <summary>
/// Represents a busy time slot from a calendar
/// </summary>
public class BusyTimeSlot
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Summary { get; set; }
}
