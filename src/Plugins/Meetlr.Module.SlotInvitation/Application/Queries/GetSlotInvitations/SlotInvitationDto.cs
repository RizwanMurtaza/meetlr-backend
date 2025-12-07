using Meetlr.Module.SlotInvitation.Domain.Enums;

namespace Meetlr.Module.SlotInvitation.Application.Queries.GetSlotInvitations;

public record SlotInvitationDto
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
    public Guid? BookingId { get; init; }
    public DateTime? BookedAt { get; init; }
    public EmailDeliveryStatus EmailStatus { get; init; }
    public int EmailAttempts { get; init; }
    public DateTime? EmailSentAt { get; init; }
    public string? EmailError { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }

    /// <summary>
    /// Whether the invitation is still valid (pending and not expired)
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Whether the invitation can be deleted (only if not booked)
    /// </summary>
    public bool CanBeDeleted { get; init; }

    /// <summary>
    /// Whether the email can be resent
    /// </summary>
    public bool CanResendEmail { get; init; }
}
