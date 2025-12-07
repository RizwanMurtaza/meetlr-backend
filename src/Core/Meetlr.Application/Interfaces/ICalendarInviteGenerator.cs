namespace Meetlr.Application.Interfaces;

/// <summary>
/// Service for generating ICS calendar invite files
/// </summary>
public interface ICalendarInviteGenerator
{
    /// <summary>
    /// Generate an ICS calendar invite file content
    /// </summary>
    byte[] GenerateIcsFile(CalendarInviteRequest request);
}

/// <summary>
/// Request for generating a calendar invite
/// </summary>
public record CalendarInviteRequest
{
    public string EventUid { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime StartTimeUtc { get; init; }
    public DateTime EndTimeUtc { get; init; }
    public string? Location { get; init; }
    public string OrganizerEmail { get; init; } = string.Empty;
    public string OrganizerName { get; init; } = string.Empty;
    public string AttendeeEmail { get; init; } = string.Empty;
    public string AttendeeName { get; init; } = string.Empty;
    public string? MeetingUrl { get; init; }
    public CalendarInviteMethod Method { get; init; } = CalendarInviteMethod.Request;

    /// <summary>
    /// Sequence number for updates (incremented on reschedule)
    /// </summary>
    public int Sequence { get; init; } = 0;
}

/// <summary>
/// Calendar invite method (REQUEST for new, CANCEL for cancellation)
/// </summary>
public enum CalendarInviteMethod
{
    Request,
    Cancel
}
