namespace Meetlr.Module.Calendar.Application.Models;

/// <summary>
/// Represents a calendar event from a provider
/// </summary>
public record CalendarEvent
{
    /// <summary>
    /// Event ID from the provider
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Calendar ID this event belongs to
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
    public string? TimeZone { get; init; }

    /// <summary>
    /// Event location (physical address or virtual meeting link)
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// Whether this is an all-day event
    /// </summary>
    public bool IsAllDay { get; init; }

    /// <summary>
    /// Event status (confirmed, tentative, cancelled)
    /// </summary>
    public string? Status { get; init; }

    /// <summary>
    /// Organizer email
    /// </summary>
    public string? OrganizerEmail { get; init; }

    /// <summary>
    /// Attendee emails
    /// </summary>
    public List<string> Attendees { get; init; } = new();

    /// <summary>
    /// Video conferencing link if available
    /// </summary>
    public string? MeetingLink { get; init; }

    /// <summary>
    /// Whether this event blocks the user's availability
    /// </summary>
    public bool IsBusy { get; init; } = true;

    /// <summary>
    /// Recurrence rule (RRULE) if this is a recurring event
    /// </summary>
    public string? RecurrenceRule { get; init; }
}
