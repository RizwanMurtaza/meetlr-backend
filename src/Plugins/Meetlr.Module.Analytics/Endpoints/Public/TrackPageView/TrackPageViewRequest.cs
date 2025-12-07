using Meetlr.Module.Analytics.Application.Commands.TrackPageView;

namespace Meetlr.Module.Analytics.Endpoints.Public.TrackPageView;

/// <summary>
/// API request model for tracking a page view
/// </summary>
public record TrackPageViewRequest
{
    /// <summary>
    /// Type of page viewed: "homepage", "eventList", or "eventPage"
    /// </summary>
    public string PageType { get; init; } = string.Empty;

    /// <summary>
    /// Session ID for unique visitor tracking
    /// </summary>
    public string SessionId { get; init; } = string.Empty;

    /// <summary>
    /// Browser user agent string
    /// </summary>
    public string? UserAgent { get; init; }

    /// <summary>
    /// Referring URL
    /// </summary>
    public string? Referrer { get; init; }

    /// <summary>
    /// Convert to command (username and eventSlug come from route parameters)
    /// </summary>
    public TrackPageViewCommand ToCommand(string username, string? eventSlug, string? ipAddress) => new()
    {
        PageType = PageType,
        Username = username,
        EventSlug = eventSlug,
        SessionId = SessionId,
        UserAgent = UserAgent,
        Referrer = Referrer,
        IpAddress = ipAddress
    };
}
