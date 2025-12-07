using Meetlr.Module.Analytics.Application.Common.Models;
using Meetlr.Module.Analytics.Application.Queries.GetUserAnalytics;

namespace Meetlr.Module.Analytics.Endpoints.GetUserAnalytics;

/// <summary>
/// API response model for user analytics dashboard
/// </summary>
public record GetUserAnalyticsResponse
{
    public int TotalViews { get; init; }
    public int UniqueVisitors { get; init; }
    public int TotalBookings { get; init; }
    public decimal ConversionRate { get; init; }
    public List<DailyViews> ViewsOverTime { get; init; } = new();
    public List<TopEvent> TopEvents { get; init; } = new();
    public Dictionary<string, int> ViewsByPageType { get; init; } = new();
    public Dictionary<string, int> ViewsByDevice { get; init; } = new();

    /// <summary>
    /// Create from query response
    /// </summary>
    public static GetUserAnalyticsResponse FromQueryResponse(GetUserAnalyticsQueryResponse queryResponse) => new()
    {
        TotalViews = queryResponse.TotalViews,
        UniqueVisitors = queryResponse.UniqueVisitors,
        TotalBookings = queryResponse.TotalBookings,
        ConversionRate = queryResponse.ConversionRate,
        ViewsOverTime = queryResponse.ViewsOverTime,
        TopEvents = queryResponse.TopEvents,
        ViewsByPageType = queryResponse.ViewsByPageType,
        ViewsByDevice = queryResponse.ViewsByDevice
    };
}
