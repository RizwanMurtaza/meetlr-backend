namespace Meetlr.Module.Calendar.Application.Models;

/// <summary>
/// Information about a calendar
/// </summary>
public record CalendarInfo
{
    /// <summary>
    /// Calendar ID from the provider
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Calendar name/title
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Whether this is the primary calendar
    /// </summary>
    public bool IsPrimary { get; init; }

    /// <summary>
    /// Calendar color (hex)
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    /// Calendar description
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Calendar timezone
    /// </summary>
    public string? TimeZone { get; init; }

    /// <summary>
    /// Access role (owner, reader, writer)
    /// </summary>
    public string? AccessRole { get; init; }
}
