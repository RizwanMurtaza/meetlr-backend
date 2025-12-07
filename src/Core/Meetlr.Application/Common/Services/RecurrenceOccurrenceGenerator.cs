using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Events;

namespace Meetlr.Application.Common.Services;

/// <summary>
/// DEPRECATED: No longer used with simplified series model.
/// Series now use explicitly selected date times instead of generating from recurrence patterns.
/// This class is kept for backward compatibility but should not be used.
/// </summary>
[Obsolete("RecurrenceOccurrenceGenerator is deprecated. Use selected date times instead of recurrence patterns.")]
public class RecurrenceOccurrenceGenerator : IRecurrenceOccurrenceGenerator
{
    public List<DateTime> GenerateOccurrences(BookingSeries series, DateTime startDate, int maxOccurrences)
    {
        throw new NotSupportedException(
            "RecurrenceOccurrenceGenerator is no longer supported. " +
            "Use explicitly selected date times instead of recurrence patterns.");
    }
}
