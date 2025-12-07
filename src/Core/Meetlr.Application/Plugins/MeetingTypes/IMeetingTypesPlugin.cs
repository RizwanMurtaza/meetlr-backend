using Meetlr.Application.Plugins.MeetingTypes.Models;
using Meetlr.Domain.Enums;

namespace Meetlr.Application.Plugins.MeetingTypes;

/// <summary>
/// Interface for meeting types plugins (Zoom, Google Meet, Microsoft Teams, Jitsi, etc.)
/// Extends IPlugin with video meeting operations.
/// Connection methods (GenerateConnectUrl, CompleteConnect, Disconnect) are in base IPlugin.
/// </summary>
public interface IMeetingTypesPlugin : IPlugin
{
    /// <summary>
    /// The meeting location type this plugin handles
    /// </summary>
    MeetingLocationType LocationType { get; }

    /// <summary>
    /// Check if this meeting type is available for a specific user
    /// (e.g., has connected their account, has valid credentials)
    /// </summary>
    Task<bool> IsAvailableForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a video meeting
    /// </summary>
    Task<VideoMeetingResult> CreateMeetingAsync(
        CreateMeetingRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a video meeting (when booking is cancelled)
    /// </summary>
    Task<bool> DeleteMeetingAsync(
        string meetingId,
        Guid userId,
        CancellationToken cancellationToken = default);
}
