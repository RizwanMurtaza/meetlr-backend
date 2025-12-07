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
/// Queues all tasks when a booking is rescheduled:
/// 1. Calendar reschedule (delete old event + create new one with updated times)
/// 2. Reschedule notification email (shows old vs new time)
///
/// Tasks are queued with staggered ExecuteAt times to ensure sequential processing:
/// - CalendarReschedule: Now (handles delete old + create new)
/// - Email notification: Now + 5 seconds (after calendar is updated)
/// </summary>
public class QueueBookingRescheduledTasksHandler : INotificationHandler<BookingRescheduledEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationQueueService _notificationQueueService;
    private readonly ILogger<QueueBookingRescheduledTasksHandler> _logger;

    public QueueBookingRescheduledTasksHandler(
        IUnitOfWork unitOfWork,
        INotificationQueueService notificationQueueService,
        ILogger<QueueBookingRescheduledTasksHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationQueueService = notificationQueueService;
        _logger = logger;
    }

    public async Task Handle(BookingRescheduledEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Queuing tasks for rescheduled booking {BookingId}. " +
                "Old: {OldStartTime} - {OldEndTime}, New: {NewStartTime} - {NewEndTime}",
                notification.BookingId,
                notification.OldStartTime,
                notification.OldEndTime,
                notification.NewStartTime,
                notification.NewEndTime);

            // Load booking with related data
            var booking = await _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .Include(b => b.MeetlrEvent)
                .Include(b => b.Contact)
                .FirstOrDefaultAsync(b => b.Id == notification.BookingId, cancellationToken);

            if (booking == null)
            {
                _logger.LogWarning(
                    "Booking {BookingId} not found for reschedule task queuing",
                    notification.BookingId);
                return;
            }

            var now = DateTime.UtcNow;

            // 1. Queue calendar reschedule (delete old event + create new one)
            // This handles updating the calendar with the new times
            if (!string.IsNullOrEmpty(booking.CalendarEventId))
            {
                await _notificationQueueService.QueueTaskAsync(
                    booking.Id,
                    booking.MeetlrEventId,
                    notification.HostUserId,
                    notification.TenantId,
                    NotificationType.CalendarReschedule,
                    executeAt: now,
                    cancellationToken);

                _logger.LogDebug("Queued CalendarReschedule for booking {BookingId}", booking.Id);
            }

            // 2. Queue reschedule notification email
            // Uses the new method that passes old times in payload
            await _notificationQueueService.QueueRescheduleNotificationAsync(
                booking,
                booking.MeetlrEvent,
                notification.OldStartTime,
                notification.OldEndTime,
                cancellationToken);

            _logger.LogDebug("Queued reschedule notifications for booking {BookingId}", booking.Id);

            _logger.LogInformation(
                "Successfully queued reschedule tasks for booking {BookingId}",
                notification.BookingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to queue reschedule tasks for booking {BookingId}",
                notification.BookingId);
            // Don't rethrow - task queuing failure shouldn't break the reschedule flow
        }
    }
}
