namespace Meetlr.Application.Features.Availability.Queries.GetUserBookingSeries;

public record GetUserBookingSeriesQueryResponse
{
    public List<BookingSeriesSummaryDto> Series { get; init; } = new();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}
