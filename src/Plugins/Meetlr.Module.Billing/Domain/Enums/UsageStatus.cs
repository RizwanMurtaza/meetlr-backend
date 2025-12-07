namespace Meetlr.Module.Billing.Domain.Enums;

/// <summary>
/// Status of a credit usage record.
/// Used to track the lifecycle of credit reservations.
/// </summary>
public enum UsageStatus
{
    /// <summary>
    /// Credits have been reserved (deducted) but notification not yet sent.
    /// </summary>
    Reserved = 0,

    /// <summary>
    /// Notification sent successfully, credits confirmed as used.
    /// </summary>
    Confirmed = 1,

    /// <summary>
    /// Notification send failed, credits have been refunded to user.
    /// </summary>
    Refunded = 2
}
