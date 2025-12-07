namespace Meetlr.Application.Features.Dashboard.Queries.GetDashboardStats;

public class GetDashboardStatsQueryResponse
{
    public int EventTypesCount { get; set; }
    public int UpcomingBookingsCount { get; set; }
    public int TotalBookingsCount { get; set; }
}
