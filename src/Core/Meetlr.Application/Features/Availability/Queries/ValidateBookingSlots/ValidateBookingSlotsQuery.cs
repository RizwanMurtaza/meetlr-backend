using MediatR;

namespace Meetlr.Application.Features.Availability.Queries.ValidateBookingSlots;

/// <summary>
/// Unified query for validating one or more booking time slots
/// Handles both single bookings and recurring series
/// Checks meeting type (OneOnOne vs Group) and capacity
/// </summary>
public class ValidateBookingSlotsQuery : IRequest<ValidateBookingSlotsResponse>
{
    public Guid MeetlrEventId { get; set; }

    /// <summary>
    /// List of UTC date/times to validate (can be single item for one booking, or multiple for series)
    /// </summary>
    public List<DateTime> RequestedSlots { get; set; } = new();

    public string TimeZone { get; set; } = string.Empty;

    /// <summary>
    /// When booking via a slot invitation, exclude that invitation from the reservation check.
    /// This prevents the invitation's own reserved slot from blocking the booking.
    /// </summary>
    public string? SlotInvitationTokenToExclude { get; set; }
}
