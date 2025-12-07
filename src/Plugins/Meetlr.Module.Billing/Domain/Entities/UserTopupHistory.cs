using Meetlr.Domain.Common;
using Meetlr.Module.Billing.Domain.Enums;

namespace Meetlr.Module.Billing.Domain.Entities;

/// <summary>
/// Records credit additions (top-ups, package grants, refunds, etc.)
/// Provides an audit trail of all credit additions.
/// </summary>
public class UserTopupHistory : BaseAuditableEntity
{
    /// <summary>
    /// The user who received the credits
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Number of credits added
    /// </summary>
    public int CreditsAdded { get; set; }

    /// <summary>
    /// Amount paid for this top-up (0 for package grants, refunds, etc.)
    /// </summary>
    public decimal AmountPaid { get; set; }

    /// <summary>
    /// Currency of the payment
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Type of transaction that added these credits
    /// </summary>
    public TransactionType TransactionType { get; set; }

    /// <summary>
    /// Stripe PaymentIntent ID for payment tracking
    /// </summary>
    public string? StripePaymentIntentId { get; set; }

    /// <summary>
    /// Human-readable description of the transaction
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// User's credit balance after this transaction (for audit trail)
    /// </summary>
    public int BalanceAfter { get; set; }

    /// <summary>
    /// Reference to related package subscription (if from a package)
    /// </summary>
    public Guid? UserPackageId { get; set; }

    // Navigation Properties

    /// <summary>
    /// Related package subscription (if applicable)
    /// </summary>
    public UserPackage? UserPackage { get; set; }
}
