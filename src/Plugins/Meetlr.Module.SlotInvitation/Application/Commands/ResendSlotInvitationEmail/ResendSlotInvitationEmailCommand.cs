using MediatR;

namespace Meetlr.Module.SlotInvitation.Application.Commands.ResendSlotInvitationEmail;

/// <summary>
/// Command to resend the invitation email for a slot invitation.
/// Limited to 3 total attempts per invitation.
/// </summary>
public record ResendSlotInvitationEmailCommand : IRequest<ResendSlotInvitationEmailCommandResponse>
{
    public Guid SlotInvitationId { get; init; }
}

public record ResendSlotInvitationEmailCommandResponse
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public int EmailAttempts { get; init; }
    public bool CanResendAgain { get; init; }
}
