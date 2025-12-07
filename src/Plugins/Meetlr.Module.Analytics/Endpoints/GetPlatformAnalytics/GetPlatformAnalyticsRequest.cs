namespace Meetlr.Module.Analytics.Endpoints.GetPlatformAnalytics;

/// <summary>
/// API request model for getting platform analytics
/// </summary>
public record GetPlatformAnalyticsRequest
{
    /// <summary>
    /// Period filter: "7d", "30d", "90d", or "all"
    /// </summary>
    public string Period { get; init; } = "30d";
}
