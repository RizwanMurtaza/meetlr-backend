namespace Meetlr.Module.SlotInvitation.Domain.Enums;

/// <summary>
/// Status of a slot invitation
/// </summary>
public enum SlotInvitationStatus
{
    /// <summary>
    /// Invitation created, waiting for invitee to book
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Invitee completed the booking
    /// </summary>
    Booked = 1,

    /// <summary>
    /// Invitation expired without being booked
    /// </summary>
    Expired = 2,

    /// <summary>
    /// Host cancelled/deleted the invitation
    /// </summary>
    Cancelled = 3
}
