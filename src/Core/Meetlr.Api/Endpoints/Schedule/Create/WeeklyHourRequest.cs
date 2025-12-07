using Meetlr.Domain.Enums;

namespace Meetlr.Api.Endpoints.Schedule.Create;

public class WeeklyHourRequest
{
    public DayOfWeekEnum DayOfWeek { get; init; }
    public TimeSpan StartTime { get; init; }
    public TimeSpan EndTime { get; init; }
    public bool IsAvailable { get; init; } = true;
}
