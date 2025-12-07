using MediatR;
using Meetlr.Domain.Enums;

namespace Meetlr.Application.Features.Bookings.Commands.UpdateSeriesBooking;

public record UpdateSeriesBookingCommand : IRequest<UpdateSeriesBookingCommandResponse>
{
    public Guid SeriesId { get; init; }
    public CancellationScope Scope { get; init; }
    public DateTime? NewStartTime { get; init; }
    public int? NewDuration { get; init; }
    public Guid? StartFromBookingId { get; init; }
}
