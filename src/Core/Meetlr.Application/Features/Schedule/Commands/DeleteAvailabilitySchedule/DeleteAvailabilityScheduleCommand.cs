using MediatR;

namespace Meetlr.Application.Features.Schedule.Commands.DeleteAvailabilitySchedule;

public class DeleteAvailabilityScheduleCommand : IRequest<DeleteAvailabilityScheduleCommandResponse>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
}
