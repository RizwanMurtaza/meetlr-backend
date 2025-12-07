using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Application.Features.Bookings.EventHandlers;

/// <summary>
/// Queues all tasks when a booking is cancelled:
/// 1. Video meeting deletion (if exists)
/// 2. Calendar event deletion (if exists)
/// 3. Refund processing (if payment was completed)
/// 4. Cancellation notification
///
/// Tasks are queued with staggered ExecuteAt times to ensure sequential processing:
/// - VideoLinkDeletion: Now
/// - CalendarDeletion: Now + 2 seconds
/// - Refund: Now + 3 seconds (if applicable)
/// - Email notifications: Now + 5 seconds
/// </summary>
public class QueueBookingCancelledTasksHandler : INotificationHandler<BookingCancelledEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationQueueService _notificationQueueService;
    private readonly ILogger<QueueBookingCancelledTasksHandler> _logger;

    public QueueBookingCancelledTasksHandler(
        IUnitOfWork unitOfWork,
        INotificationQueueService notificationQueueService,
        ILogger<QueueBookingCancelledTasksHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationQueueService = notificationQueueService;
        _logger = logger;
    }

    public async Task Handle(BookingCancelledEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Queuing tasks for cancelled booking {BookingId}",
                notification.BookingId);

            // Load booking with related data
            var booking = await _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .Include(b => b.MeetlrEvent)
                .Include(b => b.Contact)
                .FirstOrDefaultAsync(b => b.Id == notification.BookingId, cancellationToken);

            if (booking == null)
            {
                _logger.LogWarning(
                    "Booking {BookingId} not found for cancellation task queuing",
                    notification.BookingId);
                return;
            }

            var now = DateTime.UtcNow;

            // 1. Queue video meeting deletion (if meeting exists)
            if (!string.IsNullOrEmpty(booking.MeetingId))
            {
                await _notificationQueueService.QueueTaskAsync(
                    booking.Id,
                    booking.MeetlrEventId,
                    notification.HostUserId,
                    notification.TenantId,
                    NotificationType.VideoLinkDeletion,
                    executeAt: now,
                    cancellationToken);

                _logger.LogDebug("Queued VideoLinkDeletion for booking {BookingId}", booking.Id);
            }

            // 2. Queue calendar event deletion (if calendar event exists)
            if (!string.IsNullOrEmpty(booking.CalendarEventId))
            {
                await _notificationQueueService.QueueTaskAsync(
                    booking.Id,
                    booking.MeetlrEventId,
                    notification.HostUserId,
                    notification.TenantId,
                    NotificationType.CalendarDeletion,
                    executeAt: now.AddSeconds(2),
                    cancellationToken);

                _logger.LogDebug("Queued CalendarDeletion for booking {BookingId}", booking.Id);
            }

            // 3. Queue refund (if payment was completed)
            if (booking.PaymentStatus == PaymentStatus.Completed &&
                !string.IsNullOrEmpty(booking.PaymentIntentId))
            {
                // Mark as refund pending
                booking.PaymentStatus = PaymentStatus.RefundPending;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Queue refund for background processing
                await _notificationQueueService.QueueRefundAsync(booking, cancellationToken);

                _logger.LogDebug(
                    "Queued refund for booking {BookingId}, PaymentIntentId: {PaymentIntentId}",
                    booking.Id, booking.PaymentIntentId);
            }

            // 4. Queue cancellation notifications (email/SMS/WhatsApp)
            await _notificationQueueService.QueueBookingNotificationsAsync(
                booking,
                booking.MeetlrEvent,
                NotificationTrigger.BookingCancelled,
                cancellationToken);

            _logger.LogInformation(
                "Successfully queued all cancellation tasks for booking {BookingId}. " +
                "HadMeeting: {HadMeeting}, HadCalendar: {HadCalendar}, RefundQueued: {RefundQueued}",
                notification.BookingId,
                !string.IsNullOrEmpty(booking.MeetingId),
                !string.IsNullOrEmpty(booking.CalendarEventId),
                booking.PaymentStatus == PaymentStatus.RefundPending);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to queue cancellation tasks for booking {BookingId}",
                notification.BookingId);
            // Don't rethrow - task queuing failure shouldn't break the cancellation flow
        }
    }
}
