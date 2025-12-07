using Meetlr.Application.Features.Bookings.Queries.VerifyBookingIdentity;

namespace Meetlr.Api.Endpoints.Bookings.Reschedule;

public class VerifyIdentityResponse
{
    public bool Success { get; init; }
    public bool CanReschedule { get; init; }
    public string? BlockedReason { get; init; }

    // Booking details
    public Guid BookingId { get; init; }
    public DateTime CurrentStartTime { get; init; }
    public DateTime CurrentEndTime { get; init; }
    public int RescheduleCount { get; init; }

    // Event details for showing the calendar
    public Guid MeetlrEventId { get; init; }
    public string EventName { get; init; } = string.Empty;
    public int DurationMinutes { get; init; }
    public string HostName { get; init; } = string.Empty;
    public string HostUsername { get; init; } = string.Empty;

    // Guest info
    public string GuestName { get; init; } = string.Empty;
    public string GuestEmail { get; init; } = string.Empty;

    public static VerifyIdentityResponse FromQueryResponse(VerifyBookingIdentityQueryResponse response)
    {
        return new VerifyIdentityResponse
        {
            Success = response.Success,
            CanReschedule = response.CanReschedule,
            BlockedReason = response.BlockedReason,
            BookingId = response.BookingId,
            CurrentStartTime = response.CurrentStartTime,
            CurrentEndTime = response.CurrentEndTime,
            RescheduleCount = response.RescheduleCount,
            MeetlrEventId = response.MeetlrEventId,
            EventName = response.EventName,
            DurationMinutes = response.DurationMinutes,
            HostName = response.HostName,
            HostUsername = response.HostUsername,
            GuestName = response.GuestName,
            GuestEmail = response.GuestEmail
        };
    }
}
