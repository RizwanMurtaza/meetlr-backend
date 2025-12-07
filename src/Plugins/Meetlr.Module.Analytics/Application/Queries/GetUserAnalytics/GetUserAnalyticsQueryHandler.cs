using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Module.Analytics.Application.Common.Models;
using Meetlr.Module.Analytics.Domain.Entities;
using Meetlr.Module.Analytics.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.Analytics.Application.Queries.GetUserAnalytics;

/// <summary>
/// Handler for getting user analytics dashboard data
/// </summary>
public class GetUserAnalyticsQueryHandler : IRequestHandler<GetUserAnalyticsQuery, GetUserAnalyticsQueryResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserAnalyticsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetUserAnalyticsQueryResponse> Handle(GetUserAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var startDate = GetStartDate(request.Period);

        // Get all page views for this user in the period
        var pageViewsQuery = _unitOfWork.Repository<PageView>()
            .GetQueryable()
            .Where(p => p.Username == request.Username);

        if (startDate.HasValue)
        {
            pageViewsQuery = pageViewsQuery.Where(p => p.ViewedAt >= startDate.Value);
        }

        var pageViews = await pageViewsQuery.ToListAsync(cancellationToken);

        // Calculate metrics
        var totalViews = pageViews.Count;
        var uniqueVisitors = pageViews
            .Where(p => !string.IsNullOrEmpty(p.SessionId))
            .Select(p => p.SessionId)
            .Distinct()
            .Count();

        // Views by page type
        var viewsByPageType = pageViews
            .GroupBy(p => p.PageType)
            .ToDictionary(
                g => g.Key.ToString(),
                g => g.Count()
            );

        // Views by device
        var viewsByDevice = pageViews
            .Where(p => !string.IsNullOrEmpty(p.DeviceType))
            .GroupBy(p => p.DeviceType!)
            .ToDictionary(
                g => g.Key,
                g => g.Count()
            );

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

        // Top events by views
        var topEvents = pageViews
            .Where(p => p.PageType == PageViewType.EventPage && !string.IsNullOrEmpty(p.EventSlug))
            .GroupBy(p => p.EventSlug!)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => new TopEvent
            {
                EventSlug = g.Key,
                EventName = g.Key, // Will be resolved by frontend or enriched later
                Views = g.Count()
            })
            .ToList();

        // Calculate conversion rate (placeholder - would need booking data)
        // For now, return 0 - this would be enriched when we have booking correlation
        var conversionRate = 0m;
        var totalBookings = 0;

        return new GetUserAnalyticsQueryResponse
        {
            TotalViews = totalViews,
            UniqueVisitors = uniqueVisitors,
            TotalBookings = totalBookings,
            ConversionRate = conversionRate,
            ViewsOverTime = viewsOverTime,
            TopEvents = topEvents,
            ViewsByPageType = viewsByPageType,
            ViewsByDevice = viewsByDevice
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
