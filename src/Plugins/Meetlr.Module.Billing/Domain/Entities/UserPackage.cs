using Meetlr.Domain.Common;
using Meetlr.Module.Billing.Domain.Enums;

namespace Meetlr.Module.Billing.Domain.Entities;

/// <summary>
/// Represents a user's subscription to a package.
/// Tracks the current billing period and usage.
/// </summary>
public class UserPackage : BaseAuditableEntity
{
    /// <summary>
    /// The user who subscribed to this package
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The package they subscribed to
    /// </summary>
    public Guid PackageId { get; set; }

    /// <summary>
    /// Current status of the subscription
    /// </summary>
    public PackageStatus Status { get; set; } = PackageStatus.Active;

    /// <summary>
    /// Start date of the current billing period
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date of the current billing period
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Credits granted at the start of this billing period
    /// </summary>
    public int CreditsGranted { get; set; }

    /// <summary>
    /// Credits consumed during this billing period
    /// </summary>
    public int CreditsUsed { get; set; }

    /// <summary>
    /// Stripe subscription ID for recurring billing
    /// </summary>
    public string? StripeSubscriptionId { get; set; }

    /// <summary>
    /// When the subscription was cancelled (if cancelled)
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// Reason for cancellation (optional)
    /// </summary>
    public string? CancellationReason { get; set; }

    // Navigation Properties

    /// <summary>
    /// The package definition
    /// </summary>
    public Package Package { get; set; } = null!;

    // Helper Methods

    /// <summary>
    /// Check if the subscription is currently active
    /// </summary>
    public bool IsActive => Status == PackageStatus.Active && EndDate > DateTime.UtcNow;

    /// <summary>
    /// Check if the subscription has expired
    /// </summary>
    public bool IsExpired => EndDate <= DateTime.UtcNow;

    /// <summary>
    /// Get remaining credits for this period
    /// </summary>
    public int RemainingCredits => Math.Max(0, CreditsGranted - CreditsUsed);

    /// <summary>
    /// Mark the subscription as cancelled
    /// </summary>
    public void Cancel(string? reason = null)
    {
        Status = PackageStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;
    }

    /// <summary>
    /// Mark the subscription as expired
    /// </summary>
    public void Expire()
    {
        if (Status == PackageStatus.Active)
        {
            Status = PackageStatus.Expired;
        }
    }
}
