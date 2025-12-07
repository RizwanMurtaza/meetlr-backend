namespace Meetlr.Application.Features.Bookings.Commands.PauseSeriesBooking;

public record PauseSeriesBookingCommandResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string NewStatus { get; init; } = string.Empty;
}
