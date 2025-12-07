using Meetlr.Application.Features.Availability.Queries.GetAvailableSlots;
using Meetlr.Domain.Enums;

namespace Meetlr.Api.Endpoints.Public.GetAvailableSlots;

public class GetAvailableSlotsResponse
{
    public List<AvailableSlot> Slots { get; init; } = new();
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

    public static GetAvailableSlotsResponse FromQueryResponse(GetAvailableSlotsQueryResponse queryResponse)
    {
        return new GetAvailableSlotsResponse
        {
            Slots = queryResponse.Slots.Select(s => new AvailableSlot
            {
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                IsAvailable = s.IsAvailable,
                IsFullDay = s.IsFullDay,
                CurrentBookings = s.CurrentBookings,
                MaxCapacity = s.MaxCapacity,
                RemainingSpots = s.RemainingSpots
            }).ToList(),
            TimeZone = queryResponse.TimeZone,
            MeetingType = queryResponse.MeetingType,
            IsFullDayEvent = queryResponse.IsFullDayEvent,
            MaxAttendeesPerSlot = queryResponse.MaxAttendeesPerSlot,
            DurationMinutes = queryResponse.DurationMinutes
        };
    }
}
