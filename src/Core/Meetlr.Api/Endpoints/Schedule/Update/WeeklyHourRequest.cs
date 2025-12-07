using Meetlr.Domain.Enums;

namespace Meetlr.Api.Endpoints.Schedule.Update;

public class WeeklyHourRequest
{
    public DayOfWeekEnum DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; }
}
