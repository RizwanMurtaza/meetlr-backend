namespace Meetlr.Module.Calendar.Application.Models;

/// <summary>
/// Request to update a calendar event
/// </summary>
public record UpdateCalendarEventRequest
{
    /// <summary>
    /// Event title/summary
    /// </summary>
    public string? Summary { get; init; }

    /// <summary>
    /// Event description
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Event start time
    /// </summary>
    public DateTime? StartTime { get; init; }

    /// <summary>
    /// Event end time
    /// </summary>
    public DateTime? EndTime { get; init; }

    /// <summary>
    /// Time zone of the event
    /// </summary>
    public string? TimeZone { get; init; }

    /// <summary>
    /// Event location
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// List of attendee emails
    /// </summary>
    public List<string>? Attendees { get; init; }

    /// <summary>
    /// Video conferencing link
    /// </summary>
    public string? MeetingLink { get; init; }

    /// <summary>
    /// Whether to send update notifications to attendees
    /// </summary>
    public bool SendNotifications { get; init; } = true;
}
