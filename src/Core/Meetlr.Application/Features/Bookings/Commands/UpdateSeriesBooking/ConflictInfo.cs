namespace Meetlr.Application.Features.Bookings.Commands.UpdateSeriesBooking;

public record ConflictInfo
{
    public Guid BookingId { get; init; }
    public DateTime RequestedTime { get; init; }
    public string Reason { get; init; } = string.Empty;
}
