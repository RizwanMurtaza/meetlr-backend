using Meetlr.Domain.Enums;

namespace Meetlr.Api.Endpoints.Schedule.Get;

public class WeeklyHourItem
{
    public Guid Id { get; set; }
    public DayOfWeekEnum DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; }
}
