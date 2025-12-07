using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Notifications;
using Meetlr.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Scheduler.Services.Notifications;

/// <summary>
/// Manages moving notifications from pending to history table.
/// Uses ExecuteDeleteAsync/ExecuteUpdateAsync for reliable operations regardless of EF tracking state.
/// </summary>
public class NotificationHistoryManager : INotificationHistoryManager
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NotificationHistoryManager> _logger;

    public NotificationHistoryManager(
        IUnitOfWork unitOfWork,
        ILogger<NotificationHistoryManager> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task MoveToHistoryAsync(
        NotificationPending pending,
        NotificationStatus finalStatus,
        string? messageId,
        string? errorMessage,
        DateTime startTime,
        CancellationToken cancellationToken)
    {
        var processingTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
        var pendingId = pending.Id;

        // Check if history record already exists (idempotency - in case previous attempt
        // inserted history but failed to delete from pending)
        var existingHistory = await _unitOfWork.Repository<NotificationHistory>()
            .GetByIdAsync(pendingId, cancellationToken);

        // Re-fetch the pending notification data using AsNoTracking to avoid tracking conflicts
        var pendingData = await _unitOfWork.Repository<NotificationPending>()
            .GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == pendingId, cancellationToken);

        if (pendingData == null)
        {
            _logger.LogWarning(
                "Notification {NotificationId} not found in pending table (may have been already deleted)",
                pendingId);
            return;
        }

        if (existingHistory != null)
        {
            // History already exists, just delete from pending using ExecuteDeleteAsync
            _logger.LogWarning(
                "History record already exists for notification {NotificationId}, only deleting from pending",
                pendingId);

            var deleteCount = await _unitOfWork.Repository<NotificationPending>()
                .GetQueryable()
                .Where(x => x.Id == pendingId)
                .ExecuteDeleteAsync(cancellationToken);

            _logger.LogInformation(
                "Deleted {Count} notification(s) {NotificationId} from pending (history already existed)",
                deleteCount,
                pendingId);
            return;
        }

        // Create history record using the fetched data
        var history = new NotificationHistory
        {
            Id = pendingData.Id,
            TenantId = pendingData.TenantId,
            BookingId = pendingData.BookingId,
            MeetlrEventId = pendingData.MeetlrEventId,
            UserId = pendingData.UserId,
            NotificationType = pendingData.NotificationType,
            Trigger = pendingData.Trigger,
            FinalStatus = finalStatus,
            Recipient = pendingData.Recipient,
            PayloadJson = pendingData.PayloadJson,
            RetryCount = pendingData.RetryCount,
            ErrorMessage = errorMessage ?? pendingData.ErrorMessage,
            ErrorDetails = pendingData.ErrorDetails,
            ScheduledAt = pendingData.ScheduledAt,
            SentAt = finalStatus == NotificationStatus.Sent ? DateTime.UtcNow : null,
            ProcessingStartedAt = pendingData.ProcessingStartedAt ?? DateTime.UtcNow,
            ProcessedAt = DateTime.UtcNow,
            ExternalMessageId = messageId ?? pendingData.ExternalMessageId,
            ProcessingTimeMs = processingTime
        };

        // Add history record
        _unitOfWork.Repository<NotificationHistory>().Add(history);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Delete from pending using ExecuteDeleteAsync - bypasses EF tracking entirely
        var deleted = await _unitOfWork.Repository<NotificationPending>()
            .GetQueryable()
            .Where(x => x.Id == pendingId)
            .ExecuteDeleteAsync(cancellationToken);

        _logger.LogInformation(
            "Moved notification {NotificationId} to history with status {Status}, deleted {DeleteCount} from pending",
            pendingId,
            finalStatus,
            deleted);
    }

    public async Task HandleFailureAsync(
        NotificationPending pending,
        string? errorMessage,
        DateTime startTime,
        CancellationToken cancellationToken)
    {
        var pendingId = pending.Id;

        // Get current data to determine retry count
        var pendingData = await _unitOfWork.Repository<NotificationPending>()
            .GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == pendingId, cancellationToken);

        if (pendingData == null)
        {
            _logger.LogWarning(
                "Notification {NotificationId} not found in pending table for failure handling",
                pendingId);
            return;
        }

        var newRetryCount = pendingData.RetryCount + 1;
        var maxRetriesExceeded = newRetryCount >= pendingData.MaxRetries;

        if (maxRetriesExceeded)
        {
            // Create a temporary object with updated retry count for MoveToHistoryAsync
            pendingData.RetryCount = newRetryCount;
            await MoveToHistoryAsync(
                pendingData,
                NotificationStatus.MaxRetriesExceeded,
                null,
                errorMessage,
                startTime,
                cancellationToken);
        }
        else
        {
            var backoffMinutes = Math.Pow(2, newRetryCount);
            var nextRetryAt = DateTime.UtcNow.AddMinutes(backoffMinutes);

            // Use ExecuteUpdateAsync to bypass EF tracking issues
            await _unitOfWork.Repository<NotificationPending>()
                .GetQueryable()
                .Where(x => x.Id == pendingId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(n => n.RetryCount, newRetryCount)
                    .SetProperty(n => n.ErrorMessage, errorMessage)
                    .SetProperty(n => n.Status, NotificationStatus.Failed)
                    .SetProperty(n => n.NextRetryAt, nextRetryAt),
                    cancellationToken);

            _logger.LogWarning(
                "Notification {NotificationId} failed, retry {RetryCount}/{MaxRetries} scheduled for {NextRetry}",
                pendingId,
                newRetryCount,
                pendingData.MaxRetries,
                nextRetryAt);
        }
    }
}
