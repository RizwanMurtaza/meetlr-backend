using Meetlr.Application.Features.Bookings.Commands.CancelBooking;

namespace Meetlr.Api.Endpoints.Bookings.Cancel;

public class CancelBookingResponse
{
    public Guid BookingId { get; init; }
    public bool Success { get; init; }
    public DateTime CancelledAt { get; init; }
    public bool RefundIssued { get; init; }
    public string Message { get; init; } = string.Empty;

    public static CancelBookingResponse FromCommandResponse(CancelBookingCommandResponse commandResponse)
    {
        return new CancelBookingResponse
        {
            BookingId = commandResponse.BookingId,
            Success = commandResponse.Success,
            CancelledAt = commandResponse.CancelledAt,
            RefundIssued = commandResponse.RefundIssued,
            Message = commandResponse.Message
        };
    }
}
