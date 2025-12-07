using Meetlr.Domain.Common;
using Meetlr.Domain.Enums;

namespace Meetlr.Domain.Entities.Notifications;

/// <summary>
/// CDC (Change Data Capture) table for pending notifications
/// Background service processes this table to send notifications
/// </summary>
public class NotificationPending : BaseAuditableEntity
{
    /// <summary>
    /// Associated booking ID. Null for slot invitations which have no booking yet.
    /// </summary>
    public Guid? BookingId { get; set; }
    public Guid MeetlrEventId { get; set; }
    public Guid UserId { get; set; }

    public NotificationType NotificationType { get; set; }
    public NotificationTrigger Trigger { get; set; }
    public NotificationStatus Status { get; set; }

    /// <summary>
    /// Recipient information (email, phone number, etc.)
    /// </summary>
    public string Recipient { get; set; } = string.Empty;

    /// <summary>
    /// JSON payload containing notification data
    /// </summary>
    public string PayloadJson { get; set; } = string.Empty;

    /// <summary>
    /// Current retry attempt (0-based)
    /// </summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// Maximum retries allowed (default 3)
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Error message from last failed attempt
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
    /// When the notification should be executed/sent (for scheduled notifications like reminders and follow-ups)
    /// For immediate notifications (confirmations), this equals ScheduledAt
    /// For reminders, this is (StartTime - ReminderMinutes)
    /// For follow-ups, this is (EndTime + FollowUpHours)
    /// </summary>
    public DateTime ExecuteAt { get; set; }

    /// <summary>
    /// When the notification was actually sent
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// When the notification processing started
    /// </summary>
    public DateTime? ProcessingStartedAt { get; set; }

    /// <summary>
    /// Next retry attempt time (for exponential backoff)
    /// </summary>
    public DateTime? NextRetryAt { get; set; }

    /// <summary>
    /// External provider message ID (for tracking)
    /// </summary>
    public string? ExternalMessageId { get; set; }
}
