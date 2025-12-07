using Meetlr.Domain.Common;
using Meetlr.Module.Billing.Domain.Enums;

namespace Meetlr.Module.Billing.Domain.Entities;

/// <summary>
/// Records credit deductions for service usage.
/// Provides an audit trail of all credit usage.
/// </summary>
public class UserUsageHistory : BaseAuditableEntity
{
    /// <summary>
    /// The user who used the credits
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Type of service that consumed the credits
    /// </summary>
    public ServiceType ServiceType { get; set; }

    /// <summary>
    /// Number of credits consumed
    /// </summary>
    public int CreditsUsed { get; set; }

    /// <summary>
    /// ID of the related entity (e.g., BookingId, NotificationId)
    /// Used for idempotency - prevents double charging for same notification.
    /// </summary>
    public Guid? RelatedEntityId { get; set; }

    /// <summary>
    /// Type of the related entity (e.g., "Booking", "Notification")
    /// </summary>
    public string? RelatedEntityType { get; set; }

    /// <summary>
    /// Recipient of the message (phone number or email) - for audit purposes
    /// </summary>
    public string? Recipient { get; set; }

    /// <summary>
    /// User's credit balance after this usage (for audit trail)
    /// </summary>
    public int BalanceAfter { get; set; }

    /// <summary>
    /// Timestamp when the credits were used
    /// </summary>
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether this usage was during an unlimited plan (credits logged but not deducted)
    /// </summary>
    public bool WasUnlimited { get; set; } = false;

    /// <summary>
    /// Status of the usage: Reserved (pending send), Confirmed (sent successfully), Refunded (send failed)
    /// </summary>
    public UsageStatus Status { get; set; } = UsageStatus.Confirmed;
}
