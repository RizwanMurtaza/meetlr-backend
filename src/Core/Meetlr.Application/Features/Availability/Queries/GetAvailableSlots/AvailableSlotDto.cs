namespace Meetlr.Application.Features.Availability.Queries.GetAvailableSlots;

public record AvailableSlotDto
{
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public bool IsAvailable { get; init; }

    /// <summary>
    /// For FullDay events, this represents the date only (time portion is 00:00:00)
    /// </summary>
    public bool IsFullDay { get; init; }

    /// <summary>
    /// Current booking count for this slot (for Group/FullDay events with capacity)
    /// </summary>
    public int CurrentBookings { get; init; }

    /// <summary>
    /// Maximum capacity for this slot (for Group/FullDay events)
    /// </summary>
    public int? MaxCapacity { get; init; }

    /// <summary>
    /// Remaining spots available (MaxCapacity - CurrentBookings)
    /// </summary>
    public int? RemainingSpots => MaxCapacity.HasValue ? MaxCapacity.Value - CurrentBookings : null;
}
