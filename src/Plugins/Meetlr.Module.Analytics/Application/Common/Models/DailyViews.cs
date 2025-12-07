namespace Meetlr.Module.Analytics.Application.Common.Models;

/// <summary>
/// Represents daily view statistics for charting
/// </summary>
public class DailyViews
{
    /// <summary>
    /// The date for this data point
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Total views on this date
    /// </summary>
    public int Views { get; set; }

    /// <summary>
    /// Unique visitors on this date
    /// </summary>
    public int UniqueVisitors { get; set; }
}
