using MediatR;

namespace Meetlr.Module.Analytics.Application.Commands.TrackPageView;

/// <summary>
/// Command to track a page view event
/// </summary>
public record TrackPageViewCommand : IRequest<TrackPageViewCommandResponse>
{
    /// <summary>
    /// Type of page viewed: "homepage", "eventList", or "eventPage"
    /// </summary>
    public string PageType { get; init; } = string.Empty;

    /// <summary>
    /// Username from the URL path
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// Event slug from the URL path (for event pages)
    /// </summary>
    public string? EventSlug { get; init; }

    /// <summary>
    /// Session ID for unique visitor tracking
    /// </summary>
    public string SessionId { get; init; } = string.Empty;

    /// <summary>
    /// User ID for unique visitor tracking
    /// </summary>
    public Guid UserId { get; init; } 
    /// <summary>
    /// Browser user agent string
    /// </summary>
    public string? UserAgent { get; init; }

    /// <summary>
    /// Referring URL
    /// </summary>
    public string? Referrer { get; init; }

    /// <summary>
    /// IP address (will be hashed for privacy)
    /// </summary>
    public string? IpAddress { get; init; }
}
