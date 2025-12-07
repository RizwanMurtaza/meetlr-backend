using Meetlr.Domain.Enums;

namespace Meetlr.Application.Features.Availability.Queries.GetAvailableSlots;

/// <summary>
/// Response for get available slots query
/// </summary>
public record GetAvailableSlotsQueryResponse
{
    public List<AvailableSlotDto> Slots { get; init; } = new();
    public string TimeZone { get; init; } = string.Empty;

    /// <summary>
    /// The meeting type of the event (OneOnOne, Group, FullDay, OneOff)
    /// </summary>
    public MeetingType MeetingType { get; init; }

    /// <summary>
    /// For FullDay events, this indicates that slots represent dates, not time slots
    /// </summary>
    public bool IsFullDayEvent { get; init; }

    /// <summary>
    /// Maximum attendees per slot (for Group/FullDay events)
    /// </summary>
    public int? MaxAttendeesPerSlot { get; init; }

    /// <summary>
    /// Event duration in minutes
    /// </summary>
    public int DurationMinutes { get; init; }
}

