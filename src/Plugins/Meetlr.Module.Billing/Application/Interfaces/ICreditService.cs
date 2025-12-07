using Meetlr.Module.Billing.Domain.Entities;
using Meetlr.Module.Billing.Domain.Enums;

namespace Meetlr.Module.Billing.Application.Interfaces;

/// <summary>
/// Service for managing user credits and billing operations
/// </summary>
public interface ICreditService
{
    // Balance Operations

    /// <summary>
    /// Get user's current credit balance
    /// </summary>
    Task<int> GetBalanceAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Get or create user's credit record
    /// </summary>
    Task<UserCredits> GetOrCreateUserCreditsAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Check if user has sufficient credits for a service
    /// </summary>
    Task<bool> HasSufficientCreditsAsync(Guid userId, ServiceType serviceType, CancellationToken ct = default);

    /// <summary>
    /// Check if user has an active unlimited plan
    /// </summary>
    Task<bool> IsUnlimitedAsync(Guid userId, CancellationToken ct = default);

    // Credit Operations

    /// <summary>
    /// Reserve credits for service usage BEFORE sending. Returns false if insufficient credits.
    /// This is idempotent - if credits were already reserved for this relatedEntityId, returns success with AlreadyCharged=true.
    /// If the notification was already confirmed (sent successfully), returns AlreadySent=true to prevent double-send.
    /// Credits are deducted but marked as Reserved until confirmed.
    /// </summary>
    /// <returns>
    /// Success: true if reservation succeeded or was already done
    /// AlreadyCharged: true if credits were already reserved/confirmed for this ID
    /// AlreadySent: true if the notification was already confirmed as sent (do NOT send again)
    /// </returns>
    Task<(bool Success, bool AlreadyCharged, bool AlreadySent)> ReserveCreditsAsync(
        Guid userId,
        ServiceType serviceType,
        Guid relatedEntityId,
        string? relatedEntityType = null,
        string? recipient = null,
        CancellationToken ct = default);

    /// <summary>
    /// Confirm that reserved credits were used (notification sent successfully).
    /// </summary>
    Task ConfirmCreditsUsedAsync(Guid relatedEntityId, CancellationToken ct = default);

    /// <summary>
    /// Refund reserved credits if notification sending failed.
    /// </summary>
    Task RefundCreditsAsync(Guid relatedEntityId, CancellationToken ct = default);

    /// <summary>
    /// Deduct credits for service usage. Returns false if insufficient credits.
    /// Use ReserveCreditsAsync for notifications to prevent double charging.
    /// </summary>
    [Obsolete("Use ReserveCreditsAsync for notifications to prevent double charging")]
    Task<bool> DeductCreditsAsync(
        Guid userId,
        ServiceType serviceType,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null,
        string? recipient = null,
        CancellationToken ct = default);

    /// <summary>
    /// Add credits to user's balance
    /// </summary>
    Task AddCreditsAsync(
        Guid userId,
        int amount,
        TransactionType type,
        string? description = null,
        decimal amountPaid = 0,
        string? stripePaymentIntentId = null,
        Guid? userPackageId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Top up credits with a one-off payment (works for both pay-as-you-go and subscribed users)
    /// </summary>
    Task TopupCreditsAsync(
        Guid userId,
        int amount,
        decimal amountPaid,
        string? stripePaymentIntentId = null,
        CancellationToken ct = default);

    // Credit Cost Lookup

    /// <summary>
    /// Get the credit cost for a service type
    /// </summary>
    Task<int> GetServiceCreditCostAsync(ServiceType serviceType, CancellationToken ct = default);

    // Package Operations

    /// <summary>
    /// Subscribe user to a package
    /// </summary>
    Task<UserPackage> SubscribeToPackageAsync(
        Guid userId,
        Guid packageId,
        string? stripeSubscriptionId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Cancel user's active subscription
    /// </summary>
    Task<bool> CancelSubscriptionAsync(Guid userId, string? reason = null, CancellationToken ct = default);

    /// <summary>
    /// Get user's active package subscription
    /// </summary>
    Task<UserPackage?> GetActivePackageAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Get all available packages
    /// </summary>
    Task<List<Package>> GetAvailablePackagesAsync(CancellationToken ct = default);

    /// <summary>
    /// Get user's billing summary (balance, active package, usage stats)
    /// </summary>
    Task<UserBillingSummary> GetUserBillingSummaryAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Assign the free starter package to a new user (called on signup)
    /// </summary>
    Task AssignFreePackageAsync(Guid userId, CancellationToken ct = default);
}

/// <summary>
/// Summary of user's billing status
/// </summary>
public class UserBillingSummary
{
    public int CurrentBalance { get; set; }
    public bool IsUnlimited { get; set; }
    public DateTime? UnlimitedExpiresAt { get; set; }
    public UserPackage? ActivePackage { get; set; }
    public int TotalCreditsUsedThisMonth { get; set; }
    public int TotalTopupsThisMonth { get; set; }
}
