using Meetlr.Domain.Enums;

namespace Meetlr.Application.Features.Schedule.Queries.GetAvailabilitySchedules;

public class WeeklyHourDto
{
    public Guid Id { get; set; }
    public DayOfWeekEnum DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; }
}
