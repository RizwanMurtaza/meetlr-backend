namespace Meetlr.Application.Features.Bookings.Commands.UpdateSeriesBooking;

public record UpdateSeriesBookingCommandResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public int BookingsUpdated { get; init; }
    public List<ConflictInfo>? Conflicts { get; init; }
}
