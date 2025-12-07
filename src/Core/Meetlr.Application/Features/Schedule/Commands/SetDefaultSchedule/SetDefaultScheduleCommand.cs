using MediatR;

namespace Meetlr.Application.Features.Schedule.Commands.SetDefaultSchedule;

public class SetDefaultScheduleCommand : IRequest<SetDefaultScheduleCommandResponse>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
}
