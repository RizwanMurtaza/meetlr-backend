namespace Meetlr.Module.Billing.Domain.Enums;

/// <summary>
/// Status of a user's package subscription
/// </summary>
public enum PackageStatus
{
    /// <summary>
    /// Subscription is active
    /// </summary>
    Active = 0,

    /// <summary>
    /// Subscription has expired
    /// </summary>
    Expired = 1,

    /// <summary>
    /// Subscription was cancelled by user
    /// </summary>
    Cancelled = 2,

    /// <summary>
    /// Awaiting payment confirmation
    /// </summary>
    PendingPayment = 3
}
