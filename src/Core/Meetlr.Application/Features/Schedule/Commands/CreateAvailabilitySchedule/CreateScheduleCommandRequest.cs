using MediatR;
using Meetlr.Domain.Enums;

namespace Meetlr.Application.Features.Schedule.Commands.CreateAvailabilitySchedule;

public class CreateScheduleCommandRequest : IRequest<CreateScheduleCommandResponse>
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public ScheduleType ScheduleType { get; set; } = ScheduleType.Personal;
    public int MaxBookingsPerSlot { get; set; } = 1;
    public List<WeeklyHourDto> WeeklyHours { get; set; } = new();
}
