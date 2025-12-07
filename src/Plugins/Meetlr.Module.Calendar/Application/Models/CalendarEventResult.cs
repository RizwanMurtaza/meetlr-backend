namespace Meetlr.Module.Calendar.Application.Models;

/// <summary>
/// Result of creating a calendar event
/// </summary>
public record CalendarEventResult
{
    /// <summary>
    /// Whether the operation succeeded
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Event ID from the provider (if successful)
    /// </summary>
    public string? EventId { get; init; }

    /// <summary>
    /// URL to view the event in the calendar provider
    /// </summary>
    public string? EventUrl { get; init; }

    /// <summary>
    /// Error message if operation failed
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// The created event (if successful)
    /// </summary>
    public CalendarEvent? Event { get; init; }

    public static CalendarEventResult Succeeded(string eventId, string? eventUrl = null, CalendarEvent? calendarEvent = null) =>
        new()
        {
            Success = true,
            EventId = eventId,
            EventUrl = eventUrl,
            Event = calendarEvent
        };

    public static CalendarEventResult Failed(string errorMessage) =>
        new()
        {
            Success = false,
            ErrorMessage = errorMessage
        };
}
