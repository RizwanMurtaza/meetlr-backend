using Meetlr.Application.Features.Dashboard.Queries.GetDashboardStats;

namespace Meetlr.Api.Endpoints.Dashboard.GetStats;

public class GetDashboardStatsResponse
{
    public int EventTypesCount { get; init; }
    public int UpcomingBookingsCount { get; init; }
    public int TotalBookingsCount { get; init; }

    public static GetDashboardStatsResponse FromQueryResponse(GetDashboardStatsQueryResponse response)
    {
        return new GetDashboardStatsResponse
        {
            EventTypesCount = response.EventTypesCount,
            UpcomingBookingsCount = response.UpcomingBookingsCount,
            TotalBookingsCount = response.TotalBookingsCount
        };
    }
}
