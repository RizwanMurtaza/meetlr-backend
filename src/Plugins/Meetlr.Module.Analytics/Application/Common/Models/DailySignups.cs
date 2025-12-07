namespace Meetlr.Module.Analytics.Application.Common.Models;

/// <summary>
/// Represents daily signup statistics for platform analytics
/// </summary>
public class DailySignups
{
    /// <summary>
    /// The date for this data point
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Number of new user signups on this date
    /// </summary>
    public int Signups { get; set; }

    /// <summary>
    /// Number of new bookings on this date
    /// </summary>
    public int Bookings { get; set; }
}
