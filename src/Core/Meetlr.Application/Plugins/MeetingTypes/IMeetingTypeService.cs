using Meetlr.Domain.Enums;

namespace Meetlr.Application.Plugins.MeetingTypes;

/// <summary>
/// Interface for meeting type service - used by booking handlers to create meeting links.
/// Implemented by the MeetingTypes plugin.
/// </summary>
public interface IMeetingTypeService
{
    /// <summary>
    /// Check if the given meeting location type requires a video meeting link
    /// </summary>
    bool IsVideoLocationType(MeetingLocationType locationType);

    /// <summary>
    /// Create a meeting and return the meeting link and ID
    /// </summary>
    Task<MeetingCreationResult?> CreateMeetingAsync(
        MeetingLocationType locationType,
        MeetingCreationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a meeting
    /// </summary>
    Task<bool> DeleteMeetingAsync(
        MeetingLocationType locationType,
        string meetingId,
        Guid userId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Request for creating a meeting
/// </summary>
public record MeetingCreationRequest
{
    public required string Title { get; init; }
    public required DateTime StartTime { get; init; }
    public required int DurationMinutes { get; init; }
    public required Guid UserId { get; init; }
    public required string EventSlug { get; init; }
    public Guid? BookingId { get; init; }
    public string? AttendeeEmail { get; init; }
    public string? AttendeeName { get; init; }
}

/// <summary>
/// Result of creating a meeting
/// </summary>
public record MeetingCreationResult
{
    public required string JoinUrl { get; init; }
    public required string MeetingId { get; init; }
}
