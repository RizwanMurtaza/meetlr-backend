using MediatR;

namespace Meetlr.Application.Features.Bookings.Queries.GetBookingSeries;

public record GetBookingSeriesQuery : IRequest<GetBookingSeriesQueryResponse>
{
    public Guid SeriesId { get; init; }
}
