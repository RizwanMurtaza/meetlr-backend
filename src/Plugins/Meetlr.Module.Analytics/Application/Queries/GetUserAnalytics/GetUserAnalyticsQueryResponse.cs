using Meetlr.Module.Analytics.Application.Common.Models;

namespace Meetlr.Module.Analytics.Application.Queries.GetUserAnalytics;

/// <summary>
/// Response for the GetUserAnalytics query
/// </summary>
public record GetUserAnalyticsQueryResponse
{
    /// <summary>
    /// Total page views for the period
    /// </summary>
    public int TotalViews { get; init; }

    /// <summary>
    /// Unique visitors based on session ID
    /// </summary>
    public int UniqueVisitors { get; init; }

    /// <summary>
    /// Total bookings for the period
    /// </summary>
    public int TotalBookings { get; init; }

    /// <summary>
    /// Conversion rate (bookings / views * 100)
    /// </summary>
    public decimal ConversionRate { get; init; }

    /// <summary>
    /// Daily view data for charting
    /// </summary>
    public List<DailyViews> ViewsOverTime { get; init; } = new();

    /// <summary>
    /// Top events by view count
    /// </summary>
    public List<TopEvent> TopEvents { get; init; } = new();

    /// <summary>
    /// Views broken down by page type
    /// </summary>
    public Dictionary<string, int> ViewsByPageType { get; init; } = new();

    /// <summary>
    /// Views broken down by device type
    /// </summary>
    public Dictionary<string, int> ViewsByDevice { get; init; } = new();
}
