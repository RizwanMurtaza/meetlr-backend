using Meetlr.Domain.Enums;

namespace Meetlr.Application.Features.Schedule.Commands.UpdateAvailabilitySchedule;

public class WeeklyHourDto
{
    public DayOfWeekEnum DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; }
}
