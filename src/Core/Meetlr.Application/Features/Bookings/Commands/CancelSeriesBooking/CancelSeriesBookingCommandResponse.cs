namespace Meetlr.Application.Features.Bookings.Commands.CancelSeriesBooking;

public record CancelSeriesBookingCommandResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public int BookingsCancelled { get; init; }
    public decimal? RefundAmount { get; init; }
}
