namespace Meetlr.Application.Common.Settings;

public class RecurringBookingsSettings
{
    public int MaxOccurrences { get; set; } = 10;
    public int CalendarBatchSize { get; set; } = 10;
    public int CalendarBatchDelayMs { get; set; } = 150;
    public int MaxRetryAttempts { get; set; } = 3;
}
