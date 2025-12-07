using System.Text.Json.Serialization;
using Meetlr.Domain.Common;
using Meetlr.Domain.Enums;

namespace Meetlr.Domain.Entities.Scheduling;

public class WeeklyHours : BaseEntity
{
    public Guid AvailabilityScheduleId { get; set; }
    public DayOfWeekEnum DayOfWeek { get; set; }
    public bool IsAvailable { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    // Navigation properties
    [JsonIgnore]
    public AvailabilitySchedule AvailabilitySchedule { get; set; } = null!;
}
