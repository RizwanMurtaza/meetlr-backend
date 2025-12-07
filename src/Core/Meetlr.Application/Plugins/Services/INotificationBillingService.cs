namespace Meetlr.Application.Plugins.Services;

/// <summary>
/// Result from reserving credits for a notification.
/// </summary>
public class CreditReservationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// True if credits were already charged for this notification ID.
    /// </summary>
    public bool AlreadyCharged { get; set; }

    /// <summary>
    /// True if the notification was already successfully sent (status=Confirmed).
    /// When true, the caller should NOT send the notification again.
    /// </summary>
    public bool AlreadySent { get; set; }
}

/// <summary>
/// Service for billing operations related to notifications.
/// Implemented by the Billing module if installed.
/// Uses a reserve-then-confirm pattern to prevent double charging:
/// 1. ReserveCreditsAsync - Called BEFORE sending, deducts credits and records notification ID
/// 2. ConfirmCreditsUsedAsync - Called AFTER successful send, marks reservation as confirmed
/// 3. RefundCreditsAsync - Called if sending fails, refunds the reserved credits
/// </summary>
public interface INotificationBillingService
{
    /// <summary>
    /// Reserve (deduct) credits for a notification BEFORE sending.
    /// This is idempotent - calling multiple times with the same notificationId will return AlreadyCharged=true.
    /// </summary>
    /// <param name="userId">The user sending the notification</param>
    /// <param name="notificationType">Type of notification: "Email", "SMS", or "WhatsApp"</param>
    /// <param name="notificationId">The notification ID for idempotency tracking</param>
    /// <param name="recipient">The recipient (email/phone) for audit trail</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing success status and whether already charged</returns>
    Task<CreditReservationResult> ReserveCreditsAsync(
        Guid userId,
        string notificationType,
        Guid notificationId,
        string recipient,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirm that credits were used (notification sent successfully).
    /// Called after successful send to mark the usage as confirmed.
    /// </summary>
    /// <param name="notificationId">The notification ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ConfirmCreditsUsedAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refund credits if notification sending failed.
    /// Called when sending fails to return the reserved credits to the user.
    /// </summary>
    /// <param name="notificationId">The notification ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RefundCreditsAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default);
}
