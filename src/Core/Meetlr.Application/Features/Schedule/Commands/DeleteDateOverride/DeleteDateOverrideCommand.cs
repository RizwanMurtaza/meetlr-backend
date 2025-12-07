using MediatR;

namespace Meetlr.Application.Features.Schedule.Commands.DeleteDateOverride;

public class DeleteDateOverrideCommand : IRequest<DeleteDateOverrideResponse>
{
    public Guid ScheduleId { get; set; }
    public Guid OverrideId { get; set; }
    public Guid UserId { get; set; }
}