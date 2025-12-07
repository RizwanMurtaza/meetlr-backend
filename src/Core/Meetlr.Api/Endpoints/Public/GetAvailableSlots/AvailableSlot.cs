namespace Meetlr.Api.Endpoints.Public.GetAvailableSlots;

public class AvailableSlot
{
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public bool IsAvailable { get; init; }

    /// <summary>
    /// For FullDay events, this represents a date slot (not a time slot)
    /// </summary>
    public bool IsFullDay { get; init; }

    /// <summary>
    /// Current booking count for this slot (for Group/FullDay events)
    /// </summary>
    public int CurrentBookings { get; init; }

    /// <summary>
    /// Maximum capacity for this slot (for Group/FullDay events)
    /// </summary>
    public int? MaxCapacity { get; init; }

    /// <summary>
    /// Remaining spots available
    /// </summary>
    public int? RemainingSpots { get; init; }
}
