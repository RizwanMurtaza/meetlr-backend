namespace Meetlr.Application.Plugins.MeetingTypes.Models;

/// <summary>
/// Request model for creating a video meeting
/// </summary>
public record CreateMeetingRequest
{
    /// <summary>
    /// Meeting title
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Meeting start time (UTC)
    /// </summary>
    public required DateTime StartTime { get; init; }

    /// <summary>
    /// Meeting duration in minutes
    /// </summary>
    public required int DurationMinutes { get; init; }

    /// <summary>
    /// User ID of the meeting host
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Event slug for generating unique room names
    /// </summary>
    public required string EventSlug { get; init; }

    /// <summary>
    /// Booking ID for tracking
    /// </summary>
    public Guid? BookingId { get; init; }

    /// <summary>
    /// Attendee email (optional)
    /// </summary>
    public string? AttendeeEmail { get; init; }

    /// <summary>
    /// Attendee name (optional)
    /// </summary>
    public string? AttendeeName { get; init; }
}
