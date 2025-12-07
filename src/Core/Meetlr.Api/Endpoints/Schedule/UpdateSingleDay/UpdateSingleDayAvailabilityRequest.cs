using Meetlr.Domain.Enums;

namespace Meetlr.Api.Endpoints.Schedule.UpdateSingleDay;

public class UpdateSingleDayAvailabilityRequest
{
    public DayOfWeekEnum DayOfWeek { get; set; }

    /// <summary>
    /// List of time slots for this day. If empty or null, the day is marked as unavailable.
    /// Multiple time slots allow for split availability (e.g., 9am-12pm and 2pm-5pm)
    /// </summary>
    public List<TimeSlotRequest> TimeSlots { get; set; } = new();

    /// <summary>
    /// When false, all time slots for this day will be removed (day becomes unavailable)
    /// </summary>
    public bool IsAvailable { get; set; } = true;
}

public class TimeSlotRequest
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}
