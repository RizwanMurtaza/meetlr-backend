using MediatR;

namespace Meetlr.Application.Features.Schedule.Commands.UpdateScheduleName;

public class UpdateScheduleNameCommand : IRequest<UpdateScheduleNameResponse>
{
    public Guid ScheduleId { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
}