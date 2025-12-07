using Meetlr.Module.Analytics.Application.Common.Models;

namespace Meetlr.Module.Analytics.Application.Queries.GetPlatformAnalytics;

/// <summary>
/// Response for the GetPlatformAnalytics query
/// </summary>
public record GetPlatformAnalyticsQueryResponse
{
    /// <summary>
    /// Total platform users
    /// </summary>
    public int TotalUsers { get; init; }

    /// <summary>
    /// Total bookings across the platform
    /// </summary>
    public int TotalBookings { get; init; }

    /// <summary>
    /// Total page views across the platform
    /// </summary>
    public int TotalPageViews { get; init; }

    /// <summary>
    /// New users signed up in this period
    /// </summary>
    public int NewUsersThisPeriod { get; init; }

    /// <summary>
    /// New bookings in this period
    /// </summary>
    public int NewBookingsThisPeriod { get; init; }

    /// <summary>
    /// Daily signup data for charting
    /// </summary>
    public List<DailySignups> SignupsOverTime { get; init; } = new();

    /// <summary>
    /// Daily page views for charting
    /// </summary>
    public List<DailyViews> ViewsOverTime { get; init; } = new();

    /// <summary>
    /// Most active users by page views received
    /// </summary>
    public List<TopUser> TopUsers { get; init; } = new();

    /// <summary>
    /// Google Analytics embed URL (if configured)
    /// </summary>
    public string? GoogleAnalyticsEmbedUrl { get; init; }
}

/// <summary>
/// Represents a top user by analytics metrics
/// </summary>
public record TopUser
{
    /// <summary>
    /// User ID
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Username
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// Total page views received
    /// </summary>
    public int TotalViews { get; init; }

    /// <summary>
    /// Total bookings received
    /// </summary>
    public int TotalBookings { get; init; }
}
