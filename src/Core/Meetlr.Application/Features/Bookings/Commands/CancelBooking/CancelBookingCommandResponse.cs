namespace Meetlr.Application.Features.Bookings.Commands.CancelBooking;

/// <summary>
/// Response for cancel booking command
/// </summary>
public record CancelBookingCommandResponse
{
    public Guid BookingId { get; init; }
    public bool Success { get; init; }
    public DateTime CancelledAt { get; init; }
    public bool RefundIssued { get; init; }
    public string Message { get; init; } = string.Empty;
}
