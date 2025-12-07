namespace Meetlr.Plugins.MeetingTypes.Providers.Slack;

/// <summary>
/// Configuration settings for Slack OAuth
/// </summary>
public class SlackSettings
{
    /// <summary>
    /// Slack OAuth Client ID
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Slack OAuth Client Secret
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// OAuth redirect URI
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;

    /// <summary>
    /// Slack API base URL
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://slack.com/api";

    /// <summary>
    /// Slack OAuth authorization URL
    /// </summary>
    public string AuthorizationUrl { get; set; } = "https://slack.com/oauth/v2/authorize";

    /// <summary>
    /// Slack OAuth token URL
    /// </summary>
    public string TokenUrl { get; set; } = "https://slack.com/api/oauth.v2.access";

    /// <summary>
    /// OAuth scopes required for Slack Huddles
    /// calls:read - Read huddle information
    /// calls:write - Create huddles
    /// users:read - Read user information
    /// users:read.email - Read user email
    /// </summary>
    public string Scopes { get; set; } = "calls:read,calls:write,users:read,users:read.email";
}
