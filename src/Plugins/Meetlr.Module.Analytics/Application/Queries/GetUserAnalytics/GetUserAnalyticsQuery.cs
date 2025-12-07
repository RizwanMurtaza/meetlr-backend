using MediatR;

namespace Meetlr.Module.Analytics.Application.Queries.GetUserAnalytics;

/// <summary>
/// Query to get analytics dashboard data for a specific user
/// </summary>
public record GetUserAnalyticsQuery : IRequest<GetUserAnalyticsQueryResponse>
{
    /// <summary>
    /// The user ID to get analytics for
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Username of the user (for resolving page views by username)
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// Period filter: "7d", "30d", "90d", or "all"
    /// </summary>
    public string Period { get; init; } = "30d";
}
