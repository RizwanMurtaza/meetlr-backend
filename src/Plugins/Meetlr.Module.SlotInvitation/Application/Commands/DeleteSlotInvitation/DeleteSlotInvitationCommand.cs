using MediatR;

namespace Meetlr.Module.SlotInvitation.Application.Commands.DeleteSlotInvitation;

public record DeleteSlotInvitationCommand : IRequest<DeleteSlotInvitationCommandResponse>
{
    public Guid Id { get; init; }
}
