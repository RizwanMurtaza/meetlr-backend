using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Module.Analytics.Application.Common.Models;
using Meetlr.Module.Analytics.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.Analytics.Application.Queries.GetPlatformAnalytics;

/// <summary>
/// Handler for getting platform-wide analytics (admin only)
/// </summary>
public class GetPlatformAnalyticsQueryHandler : IRequestHandler<GetPlatformAnalyticsQuery, GetPlatformAnalyticsQueryResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPlatformAnalyticsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetPlatformAnalyticsQueryResponse> Handle(GetPlatformAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var startDate = GetStartDate(request.Period);

        // Get all page views (optionally filtered by tenant)
        var pageViewsQuery = _unitOfWork.Repository<PageView>().GetQueryable();

        if (request.TenantId.HasValue)
        {
            pageViewsQuery = pageViewsQuery.Where(p => p.TenantId == request.TenantId.Value);
        }

        if (startDate.HasValue)
        {
            pageViewsQuery = pageViewsQuery.Where(p => p.ViewedAt >= startDate.Value);
        }

        var pageViews = await pageViewsQuery.ToListAsync(cancellationToken);

        // Total page views
        var totalPageViews = pageViews.Count;

        // Daily views for chart
        var viewsOverTime = pageViews
            .GroupBy(p => p.ViewedAt.Date)
            .OrderBy(g => g.Key)
            .Select(g => new DailyViews
            {
                Date = g.Key,
                Views = g.Count(),
                UniqueVisitors = g
                    .Where(p => !string.IsNullOrEmpty(p.SessionId))
                    .Select(p => p.SessionId)
                    .Distinct()
                    .Count()
            })
            .ToList();

        // Top users by views received
        var topUsers = pageViews
            .Where(p => !string.IsNullOrEmpty(p.Username))
            .GroupBy(p => p.Username)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => new TopUser
            {
                Username = g.Key,
                TotalViews = g.Count()
            })
            .ToList();

        // Note: TotalUsers, TotalBookings, SignupsOverTime would require
        // access to User and Booking entities from the main domain
        // These could be injected via a service or queried separately
        // For now, return page view analytics only

        return new GetPlatformAnalyticsQueryResponse
        {
            TotalPageViews = totalPageViews,
            ViewsOverTime = viewsOverTime,
            TopUsers = topUsers,
            // These would be populated from other sources
            TotalUsers = 0,
            TotalBookings = 0,
            NewUsersThisPeriod = 0,
            NewBookingsThisPeriod = 0,
            SignupsOverTime = new List<DailySignups>()
        };
    }

    private static DateTime? GetStartDate(string period)
    {
        return period.ToLowerInvariant() switch
        {
            "7d" => DateTime.UtcNow.AddDays(-7),
            "30d" => DateTime.UtcNow.AddDays(-30),
            "90d" => DateTime.UtcNow.AddDays(-90),
            "all" => null,
            _ => DateTime.UtcNow.AddDays(-30)
        };
    }
}
