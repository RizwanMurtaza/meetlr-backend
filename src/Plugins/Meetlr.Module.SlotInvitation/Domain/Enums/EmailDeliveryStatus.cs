namespace Meetlr.Module.SlotInvitation.Domain.Enums;

/// <summary>
/// Status of email delivery for a slot invitation
/// </summary>
public enum EmailDeliveryStatus
{
    /// <summary>
    /// Email is queued for sending
    /// </summary>
    Queued = 0,

    /// <summary>
    /// Email was sent successfully
    /// </summary>
    Sent = 1,

    /// <summary>
    /// Email sending failed
    /// </summary>
    Failed = 2
}
