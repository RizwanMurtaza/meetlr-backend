namespace Meetlr.Module.Billing.Domain.Enums;

/// <summary>
/// Type of subscription package
/// </summary>
public enum PackageType
{
    /// <summary>
    /// No subscription - pay as you go with top-ups
    /// </summary>
    PayAsYouGo = 0,

    /// <summary>
    /// Monthly subscription
    /// </summary>
    Monthly = 1,

    /// <summary>
    /// Yearly subscription
    /// </summary>
    Yearly = 2,

    /// <summary>
    /// Unlimited usage plan
    /// </summary>
    Unlimited = 3
}
