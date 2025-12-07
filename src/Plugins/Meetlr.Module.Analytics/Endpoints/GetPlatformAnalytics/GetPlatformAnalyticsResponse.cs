using Meetlr.Module.Analytics.Application.Common.Models;
using Meetlr.Module.Analytics.Application.Queries.GetPlatformAnalytics;

namespace Meetlr.Module.Analytics.Endpoints.GetPlatformAnalytics;

/// <summary>
/// API response model for platform analytics
/// </summary>
public record GetPlatformAnalyticsResponse
{
    public int TotalUsers { get; init; }
    public int TotalBookings { get; init; }
    public int TotalPageViews { get; init; }
    public int NewUsersThisPeriod { get; init; }
    public int NewBookingsThisPeriod { get; init; }
    public List<DailySignups> SignupsOverTime { get; init; } = new();
    public List<DailyViews> ViewsOverTime { get; init; } = new();
    public List<TopUserResponse> TopUsers { get; init; } = new();
    public string? GoogleAnalyticsEmbedUrl { get; init; }

    /// <summary>
    /// Create from query response
    /// </summary>
    public static GetPlatformAnalyticsResponse FromQueryResponse(GetPlatformAnalyticsQueryResponse queryResponse) => new()
    {
        TotalUsers = queryResponse.TotalUsers,
        TotalBookings = queryResponse.TotalBookings,
        TotalPageViews = queryResponse.TotalPageViews,
        NewUsersThisPeriod = queryResponse.NewUsersThisPeriod,
        NewBookingsThisPeriod = queryResponse.NewBookingsThisPeriod,
        SignupsOverTime = queryResponse.SignupsOverTime,
        ViewsOverTime = queryResponse.ViewsOverTime,
        TopUsers = queryResponse.TopUsers.Select(u => new TopUserResponse
        {
            UserId = u.UserId,
            Username = u.Username,
            TotalViews = u.TotalViews,
            TotalBookings = u.TotalBookings
        }).ToList(),
        GoogleAnalyticsEmbedUrl = queryResponse.GoogleAnalyticsEmbedUrl
    };
}

/// <summary>
/// Top user response model
/// </summary>
public record TopUserResponse
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public int TotalViews { get; init; }
    public int TotalBookings { get; init; }
}
