using Meetlr.Application.Features.Bookings.Commands.RescheduleBooking;

namespace Meetlr.Api.Endpoints.Bookings.Reschedule;

public class RescheduleBookingRequest
{
    /// <summary>
    /// Confirmation token from the booking email
    /// </summary>
    public string ConfirmationToken { get; init; } = string.Empty;

    /// <summary>
    /// Email or phone number used when creating the booking (for re-verification)
    /// </summary>
    public string Identifier { get; init; } = string.Empty;

    /// <summary>
    /// New start time for the booking (UTC)
    /// </summary>
    public DateTime NewStartTime { get; init; }

    /// <summary>
    /// New end time for the booking (UTC)
    /// </summary>
    public DateTime NewEndTime { get; init; }

    public static RescheduleBookingCommand ToCommand(RescheduleBookingRequest request, Guid bookingId)
    {
        return new RescheduleBookingCommand
        {
            BookingId = bookingId,
            ConfirmationToken = request.ConfirmationToken,
            Identifier = request.Identifier,
            NewStartTime = request.NewStartTime,
            NewEndTime = request.NewEndTime
        };
    }
}
