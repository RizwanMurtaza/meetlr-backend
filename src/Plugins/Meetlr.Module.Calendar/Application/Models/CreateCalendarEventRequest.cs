namespace Meetlr.Module.Calendar.Application.Models;

/// <summary>
/// Request to create a calendar event
/// </summary>
public record CreateCalendarEventRequest
{
    /// <summary>
    /// User ID who owns the calendar
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Optional specific calendar ID (uses primary if not specified)
    /// </summary>
    public string? CalendarId { get; init; }

    /// <summary>
    /// Event title/summary
    /// </summary>
    public required string Summary { get; init; }

    /// <summary>
    /// Event description
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Event start time
    /// </summary>
    public required DateTime StartTime { get; init; }

    /// <summary>
    /// Event end time
    /// </summary>
    public required DateTime EndTime { get; init; }

    /// <summary>
    /// Time zone of the event
    /// </summary>
    public string TimeZone { get; init; } = "UTC";

    /// <summary>
    /// Event location
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// List of attendee emails
    /// </summary>
    public List<string> Attendees { get; init; } = new();

    /// <summary>
    /// Video conferencing link to include
    /// </summary>
    public string? MeetingLink { get; init; }

    /// <summary>
    /// Whether to send notifications to attendees
    /// </summary>
    public bool SendNotifications { get; init; } = true;
}
