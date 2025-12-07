using MediatR;

namespace Meetlr.Application.Features.Dashboard.Queries.GetDashboardStats;

public class GetDashboardStatsQuery : IRequest<GetDashboardStatsQueryResponse>
{
    public Guid UserId { get; set; }
}
