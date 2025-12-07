using MediatR;

namespace Meetlr.Application.Features.Schedule.Commands.AddDateOverride;

public class AddDateOverrideCommand : IRequest<AddDateOverrideCommandResponse>
{
    public Guid AvailabilityScheduleId { get; set; }
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public bool IsAvailable { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
}
