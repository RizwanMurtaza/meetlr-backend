using MediatR;

namespace Meetlr.Application.Features.Bookings.Commands.PauseSeriesBooking;

public record PauseSeriesBookingCommand : IRequest<PauseSeriesBookingCommandResponse>
{
    public Guid SeriesId { get; init; }
    public bool Resume { get; init; } // true to resume, false to pause
}
