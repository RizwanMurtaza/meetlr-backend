using MediatR;
using Meetlr.Domain.Enums;

namespace Meetlr.Application.Features.Availability.Queries.GetUserBookingSeries;

public record GetUserBookingSeriesQuery : IRequest<GetUserBookingSeriesQueryResponse>
{
    public Guid UserId { get; init; }
    public SeriesStatus? StatusFilter { get; init; }
    public SeriesPaymentType? PaymentTypeFilter { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
