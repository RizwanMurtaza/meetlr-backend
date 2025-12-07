using MediatR;
using Meetlr.Domain.Enums;

namespace Meetlr.Application.Features.Schedule.Commands.UpdateSingleDayAvailability;

public class UpdateSingleDayCommandRequest : IRequest<UpdateSingleDayCommandResponse>
{
    public Guid ScheduleId { get; set; }
    public Guid UserId { get; set; }
    public DayOfWeekEnum DayOfWeek { get; set; }

    /// <summary>
    /// List of time slots for this day. Supports multiple slots per day.
    /// </summary>
    public List<TimeSlotDto> TimeSlots { get; set; } = new();

    /// <summary>
    /// When false, all time slots for this day will be removed (day becomes unavailable)
    /// </summary>
    public bool IsAvailable { get; set; } = true;
}

public class TimeSlotDto
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}
