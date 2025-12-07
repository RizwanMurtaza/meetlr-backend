using Meetlr.Module.Analytics.Application.Common.Models;

namespace Meetlr.Module.Analytics.Application.Queries.GetEventAnalytics;

/// <summary>
/// Response for the GetEventAnalytics query
/// </summary>
public record GetEventAnalyticsQueryResponse
{
    /// <summary>
    /// The event ID
    /// </summary>
    public Guid EventId { get; init; }

    /// <summary>
    /// Total page views for the event
    /// </summary>
    public int TotalViews { get; init; }

    /// <summary>
    /// Unique visitors based on session ID
    /// </summary>
    public int UniqueVisitors { get; init; }

    /// <summary>
    /// Total bookings for the event in the period
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
    /// Top referrers for this event
    /// </summary>
    public Dictionary<string, int> TopReferrers { get; init; } = new();

    /// <summary>
    /// Views broken down by device type
    /// </summary>
    public Dictionary<string, int> ViewsByDevice { get; init; } = new();
}
