namespace Meetlr.Module.Analytics.Application.Common.Models;

/// <summary>
/// Represents a top-performing event by views
/// </summary>
public class TopEvent
{
    /// <summary>
    /// The event ID
    /// </summary>
    public Guid EventId { get; set; }

    /// <summary>
    /// The event name/title
    /// </summary>
    public string EventName { get; set; } = string.Empty;

    /// <summary>
    /// The event slug
    /// </summary>
    public string EventSlug { get; set; } = string.Empty;

    /// <summary>
    /// Total views for this event
    /// </summary>
    public int Views { get; set; }

    /// <summary>
    /// Number of bookings for this event
    /// </summary>
    public int Bookings { get; set; }

    /// <summary>
    /// Conversion rate (bookings / views * 100)
    /// </summary>
    public decimal ConversionRate { get; set; }
}
