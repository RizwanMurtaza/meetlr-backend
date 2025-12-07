using Meetlr.Module.Analytics.Application.Common.Models;
using Meetlr.Module.Analytics.Application.Queries.GetEventAnalytics;

namespace Meetlr.Module.Analytics.Endpoints.GetEventAnalytics;

/// <summary>
/// API response model for event analytics
/// </summary>
public record GetEventAnalyticsResponse
{
    public Guid EventId { get; init; }
    public int TotalViews { get; init; }
    public int UniqueVisitors { get; init; }
    public int TotalBookings { get; init; }
    public decimal ConversionRate { get; init; }
    public List<DailyViews> ViewsOverTime { get; init; } = new();
    public Dictionary<string, int> TopReferrers { get; init; } = new();
    public Dictionary<string, int> ViewsByDevice { get; init; } = new();

    /// <summary>
    /// Create from query response
    /// </summary>
    public static GetEventAnalyticsResponse FromQueryResponse(GetEventAnalyticsQueryResponse queryResponse) => new()
    {
        EventId = queryResponse.EventId,
        TotalViews = queryResponse.TotalViews,
        UniqueVisitors = queryResponse.UniqueVisitors,
        TotalBookings = queryResponse.TotalBookings,
        ConversionRate = queryResponse.ConversionRate,
        ViewsOverTime = queryResponse.ViewsOverTime,
        TopReferrers = queryResponse.TopReferrers,
        ViewsByDevice = queryResponse.ViewsByDevice
    };
}
