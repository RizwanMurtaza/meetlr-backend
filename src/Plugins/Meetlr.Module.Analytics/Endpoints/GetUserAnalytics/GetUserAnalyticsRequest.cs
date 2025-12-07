namespace Meetlr.Module.Analytics.Endpoints.GetUserAnalytics;

/// <summary>
/// API request model for getting user analytics
/// </summary>
public record GetUserAnalyticsRequest
{
    /// <summary>
    /// Period filter: "7d", "30d", "90d", or "all"
    /// </summary>
    public string Period { get; init; } = "30d";
}
