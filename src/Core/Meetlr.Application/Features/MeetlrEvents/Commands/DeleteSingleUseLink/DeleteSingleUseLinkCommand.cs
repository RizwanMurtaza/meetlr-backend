using MediatR;

namespace Meetlr.Application.Features.MeetlrEvents.Commands.DeleteSingleUseLink;

public class DeleteSingleUseLinkCommand : IRequest<DeleteSingleUseLinkCommandResponse>
{
    public Guid Id { get; set; }
}
