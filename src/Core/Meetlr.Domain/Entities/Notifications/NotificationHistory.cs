using Meetlr.Domain.Common;
using Meetlr.Domain.Enums;

namespace Meetlr.Domain.Entities.Notifications;

/// <summary>
/// Historical record of successfully processed notifications
/// Moved from NotificationPending after processing
/// </summary>
public class NotificationHistory : BaseAuditableEntity
{
    /// <summary>
    /// Associated booking ID. Null for slot invitations which have no booking yet.
    /// </summary>
    public Guid? BookingId { get; set; }
    public Guid MeetlrEventId { get; set; }
    public Guid UserId { get; set; }

    public NotificationType NotificationType { get; set; }
    public NotificationTrigger Trigger { get; set; }
    public NotificationStatus FinalStatus { get; set; }

    /// <summary>
    /// Recipient information (email, phone number, etc.)
    /// </summary>
    public string Recipient { get; set; } = string.Empty;

    /// <summary>
    /// JSON payload containing notification data
    /// </summary>
    public string PayloadJson { get; set; } = string.Empty;

    /// <summary>
    /// Total retry attempts made
    /// </summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// Error message from last attempt (if failed)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Full error details/stack trace for debugging
    /// </summary>
    public string? ErrorDetails { get; set; }

    /// <summary>
    /// When the notification was scheduled to be sent
    /// </summary>
    public DateTime ScheduledAt { get; set; }

    /// <summary>
    /// When the notification was actually sent
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// When the notification processing started
    /// </summary>
    public DateTime ProcessingStartedAt { get; set; }

    /// <summary>
    /// When the notification processing completed (success or failure)
    /// </summary>
    public DateTime ProcessedAt { get; set; }

    /// <summary>
    /// External provider message ID (for tracking)
    /// </summary>
    public string? ExternalMessageId { get; set; }

    /// <summary>
    /// Total processing time in milliseconds
    /// </summary>
    public long ProcessingTimeMs { get; set; }
}
