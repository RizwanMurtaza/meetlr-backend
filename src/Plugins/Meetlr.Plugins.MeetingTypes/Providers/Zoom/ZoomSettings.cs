namespace Meetlr.Plugins.MeetingTypes.Providers.Zoom;

/// <summary>
/// Configuration settings for Zoom OAuth
/// </summary>
public class ZoomSettings
{
    /// <summary>
    /// Zoom OAuth Client ID
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Zoom OAuth Client Secret
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// OAuth redirect URI
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;

    /// <summary>
    /// Zoom API base URL
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://api.zoom.us/v2";

    /// <summary>
    /// Zoom OAuth authorization URL
    /// </summary>
    public string AuthorizationUrl { get; set; } = "https://zoom.us/oauth/authorize";

    /// <summary>
    /// Zoom OAuth token URL
    /// </summary>
    public string TokenUrl { get; set; } = "https://zoom.us/oauth/token";
}
