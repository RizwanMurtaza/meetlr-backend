namespace Meetlr.Module.SlotInvitation.Application.Commands.DeleteSlotInvitation;

public record DeleteSlotInvitationCommandResponse
{
    public bool Success { get; init; }
    public Guid Id { get; init; }
}
