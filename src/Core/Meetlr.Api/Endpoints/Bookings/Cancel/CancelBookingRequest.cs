using Meetlr.Application.Features.Bookings.Commands.CancelBooking;

namespace Meetlr.Api.Endpoints.Bookings.Cancel;

public class CancelBookingRequest
{
    public string? CancellationReason { get; init; }
    public string? CancellationToken { get; init; }

    public static CancelBookingCommand ToCommand(CancelBookingRequest request, Guid bookingId)
    {
        return new CancelBookingCommand
        {
            BookingId = bookingId,
            CancellationReason = request.CancellationReason,
            CancellationToken = request.CancellationToken
        };
    }
}
