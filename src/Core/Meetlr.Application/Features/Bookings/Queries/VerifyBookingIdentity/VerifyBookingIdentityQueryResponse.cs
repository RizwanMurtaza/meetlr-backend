namespace Meetlr.Application.Features.Bookings.Queries.VerifyBookingIdentity;

public class VerifyBookingIdentityQueryResponse
{
    public bool Success { get; set; }
    public bool CanReschedule { get; set; }
    public string? BlockedReason { get; set; }

    // Booking details
    public Guid BookingId { get; set; }
    public DateTime CurrentStartTime { get; set; }
    public DateTime CurrentEndTime { get; set; }
    public int RescheduleCount { get; set; }

    // Event details for availability lookup
    public Guid MeetlrEventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string? HostName { get; set; }
    public string? HostUsername { get; set; }

    // Guest details (for display)
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
}
