namespace Meetlr.Module.Analytics.Endpoints.GetEventAnalytics;

/// <summary>
/// API request model for getting event analytics
/// </summary>
public record GetEventAnalyticsRequest
{
    /// <summary>
    /// Period filter: "7d", "30d", "90d", or "all"
    /// </summary>
    public string Period { get; init; } = "30d";
}
