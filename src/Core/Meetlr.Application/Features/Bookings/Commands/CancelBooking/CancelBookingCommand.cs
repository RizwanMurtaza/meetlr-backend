using MediatR;

namespace Meetlr.Application.Features.Bookings.Commands.CancelBooking;

/// <summary>
/// Command to cancel a booking
/// </summary>
public record CancelBookingCommand : IRequest<CancelBookingCommandResponse>
{
    public Guid BookingId { get; init; }
    public string? CancellationReason { get; init; }
    public string? CancellationToken { get; init; }
}
