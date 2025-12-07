namespace Meetlr.Scheduler.Services.Notifications;

/// <summary>
/// Service responsible for processing pending notifications.
/// Handles fetching, processing, and managing notification lifecycle.
/// </summary>
public interface INotificationProcessingService
{
    /// <summary>
    /// Process all pending notifications in the queue.
    /// </summary>
    Task ProcessPendingNotificationsAsync(CancellationToken cancellationToken);
}
