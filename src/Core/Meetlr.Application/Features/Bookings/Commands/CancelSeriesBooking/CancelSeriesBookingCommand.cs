using MediatR;
using Meetlr.Domain.Enums;

namespace Meetlr.Application.Features.Bookings.Commands.CancelSeriesBooking;

public record CancelSeriesBookingCommand : IRequest<CancelSeriesBookingCommandResponse>
{
    public Guid SeriesId { get; init; }
    public CancellationScope Scope { get; init; }
    public string Reason { get; init; } = string.Empty;
    public Guid? StartFromBookingId { get; init; } // For ThisAndFuture scope
}
