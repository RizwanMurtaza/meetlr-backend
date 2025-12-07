using MediatR;

namespace Meetlr.Application.Features.MeetlrEvents.Commands.Delete;

public class DeleteMeetlrEventCommand : IRequest<DeleteMeetlrEventCommandResponse>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
}
