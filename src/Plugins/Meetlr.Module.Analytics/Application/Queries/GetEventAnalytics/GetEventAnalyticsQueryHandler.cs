using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Module.Analytics.Application.Common.Models;
using Meetlr.Module.Analytics.Domain.Entities;
using Meetlr.Module.Analytics.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.Analytics.Application.Queries.GetEventAnalytics;

/// <summary>
/// Handler for getting analytics for a specific event
/// </summary>
public class GetEventAnalyticsQueryHandler : IRequestHandler<GetEventAnalyticsQuery, GetEventAnalyticsQueryResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetEventAnalyticsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetEventAnalyticsQueryResponse> Handle(GetEventAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var startDate = GetStartDate(request.Period);

        // Get all page views for this specific event
        var pageViewsQuery = _unitOfWork.Repository<PageView>()
            .GetQueryable()
            .Where(p => p.Username == request.Username &&
                        p.EventSlug == request.EventSlug &&
                        p.PageType == PageViewType.EventPage);

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

        // Views by device
        var viewsByDevice = pageViews
            .Where(p => !string.IsNullOrEmpty(p.DeviceType))
            .GroupBy(p => p.DeviceType!)
            .ToDictionary(
                g => g.Key,
                g => g.Count()
            );

        // Top referrers
        var topReferrers = pageViews
            .Where(p => !string.IsNullOrEmpty(p.ReferrerUrl))
            .GroupBy(p => ExtractDomain(p.ReferrerUrl!))
            .OrderByDescending(g => g.Count())
            .Take(10)
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

        // Conversion rate (placeholder - would need booking correlation)
        var conversionRate = 0m;
        var totalBookings = 0;

        return new GetEventAnalyticsQueryResponse
        {
            EventId = request.EventId,
            TotalViews = totalViews,
            UniqueVisitors = uniqueVisitors,
            TotalBookings = totalBookings,
            ConversionRate = conversionRate,
            ViewsOverTime = viewsOverTime,
            TopReferrers = topReferrers,
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

    private static string ExtractDomain(string url)
    {
        try
        {
            var uri = new Uri(url);
            return uri.Host;
        }
        catch
        {
            return "Direct";
        }
    }
}
