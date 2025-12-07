using MediatR;

namespace Meetlr.Module.Analytics.Application.Queries.GetEventAnalytics;

/// <summary>
/// Query to get analytics for a specific event
/// </summary>
public record GetEventAnalyticsQuery : IRequest<GetEventAnalyticsQueryResponse>
{
    /// <summary>
    /// The event ID to get analytics for
    /// </summary>
    public Guid EventId { get; init; }

    /// <summary>
    /// The event slug (used for page view lookup)
    /// </summary>
    public string EventSlug { get; init; } = string.Empty;

    /// <summary>
    /// Username of the event owner
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// Period filter: "7d", "30d", "90d", or "all"
    /// </summary>
    public string Period { get; init; } = "30d";
}
