using Meetlr.Domain.Entities.Notifications;
using Meetlr.Domain.Enums;

namespace Meetlr.Scheduler.Services.Notifications;

/// <summary>
/// Manages moving notifications from pending to history table.
/// This is the ONLY component responsible for deleting from NotificationPending table.
/// </summary>
public interface INotificationHistoryManager
{
    /// <summary>
    /// Moves a notification from pending to history table and deletes from pending.
    /// Uses ExecuteDeleteAsync for reliable deletion regardless of EF tracking state.
    /// </summary>
    Task MoveToHistoryAsync(
        NotificationPending pending,
        NotificationStatus finalStatus,
        string? messageId,
        string? errorMessage,
        DateTime startTime,
        CancellationToken cancellationToken);

    /// <summary>
    /// Handles notification failure with retry logic.
    /// If max retries exceeded, moves to history. Otherwise, updates retry count.
    /// </summary>
    Task HandleFailureAsync(
        NotificationPending pending,
        string? errorMessage,
        DateTime startTime,
        CancellationToken cancellationToken);
}
