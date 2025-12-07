using Meetlr.Module.SlotInvitation.Domain.Enums;

namespace Meetlr.Module.SlotInvitation.Endpoints.SlotInvitations.CreateSlotInvitation;

public record CreateSlotInvitationResponse
{
    public Guid Id { get; init; }
    public Guid MeetlrEventId { get; init; }
    public Guid? ContactId { get; init; }
    public DateTime SlotStartTime { get; init; }
    public DateTime SlotEndTime { get; init; }
    public int SpotsReserved { get; init; }
    public string Token { get; init; } = string.Empty;
    public string InviteeEmail { get; init; } = string.Empty;
    public string? InviteeName { get; init; }
    public DateTime ExpiresAt { get; init; }
    public int ExpirationHours { get; init; }
    public SlotInvitationStatus Status { get; init; }
    public EmailDeliveryStatus EmailStatus { get; init; }
    public DateTime CreatedAt { get; init; }
}
