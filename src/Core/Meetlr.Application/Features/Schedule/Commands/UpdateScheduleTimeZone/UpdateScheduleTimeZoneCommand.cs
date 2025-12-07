using MediatR;

namespace Meetlr.Application.Features.Schedule.Commands.UpdateScheduleTimeZone;

public class UpdateScheduleTimeZoneCommand : IRequest<UpdateScheduleTimeZoneResponse>
{
    public Guid ScheduleId { get; set; }
    public Guid UserId { get; set; }
    public string TimeZone { get; set; } = string.Empty;
}