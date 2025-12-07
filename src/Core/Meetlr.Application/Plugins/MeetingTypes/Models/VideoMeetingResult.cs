namespace Meetlr.Application.Plugins.MeetingTypes.Models;

/// <summary>
/// Result of creating a video meeting
/// </summary>
public record VideoMeetingResult
{
    /// <summary>
    /// URL for attendees to join the meeting
    /// </summary>
    public required string JoinUrl { get; init; }

    /// <summary>
    /// Unique meeting ID from the provider (used for deletion/updates)
    /// </summary>
    public required string MeetingId { get; init; }

    /// <summary>
    /// Optional host-specific join URL (for Zoom, etc.)
    /// </summary>
    public string? HostUrl { get; init; }

    /// <summary>
    /// Meeting password if required
    /// </summary>
    public string? Password { get; init; }

    /// <summary>
    /// Dial-in information if available
    /// </summary>
    public string? DialIn { get; init; }
}
