namespace Meetlr.Module.Billing.Domain.Enums;

/// <summary>
/// Type of credit transaction
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Manual credit purchase/top-up
    /// </summary>
    Topup = 0,

    /// <summary>
    /// Credits granted from subscription package
    /// </summary>
    PackageGrant = 1,

    /// <summary>
    /// Credit deduction for service usage
    /// </summary>
    Usage = 2,

    /// <summary>
    /// Credit refund
    /// </summary>
    Refund = 3,

    /// <summary>
    /// Admin adjustment
    /// </summary>
    Adjustment = 4,

    /// <summary>
    /// Credits rolled over from previous billing period
    /// </summary>
    Rollover = 5
}
