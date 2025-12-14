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
/// Queues all tasks when a booking is completed/confirmed:
/// 1. Video meeting creation (if location type requires it)
/// 2. Calendar sync (if auto-sync enabled)
/// 3. Confirmation notification (with meeting link and notes)
/// 4. Reminder notification (scheduled before meeting)
/// 5. Follow-up notification (scheduled after meeting)
///
/// Tasks are queued with staggered ExecuteAt times to ensure sequential processing:
/// - VideoLinkCreation: Now
/// - CalendarSync: Now + 5 seconds (after video link is created)
/// - Email notifications: Now + 10 seconds (after calendar sync, includes meeting link)
/// </summary>
public class QueueBookingCompletedTasksHandler : INotificationHandler<BookingCompletedEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationQueueService _notificationQueueService;
    private readonly ILogger<QueueBookingCompletedTasksHandler> _logger;

    public QueueBookingCompletedTasksHandler(
        IUnitOfWork unitOfWork,
        INotificationQueueService notificationQueueService,
        ILogger<QueueBookingCompletedTasksHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationQueueService = notificationQueueService;
        _logger = logger;
    }

    public async Task Handle(BookingCompletedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Queuing tasks for completed booking {BookingId}",
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
                    "Booking {BookingId} not found for task queuing",
                    notification.BookingId);
                return;
            }

            var now = DateTime.UtcNow;

            // 1. Queue video meeting creation (if location type requires it)
            // This runs first - creates the meeting link
            if (IsVideoLocationRequired(booking.MeetlrEvent.MeetingLocationType))
            {
                await _notificationQueueService.QueueTaskAsync(
                    booking.Id,
                    booking.MeetlrEventId,
                    notification.HostUserId,
                    notification.TenantId,
                    NotificationType.VideoLinkCreation,
                    executeAt: now,
                    cancellationToken);

                _logger.LogDebug("Queued VideoLinkCreation for booking {BookingId}", booking.Id);
            }

            // 2. Queue calendar sync (if auto-sync enabled)
            // Runs 5 seconds after video creation to ensure meeting link is available
            await _notificationQueueService.QueueTaskAsync(
                booking.Id,
                booking.MeetlrEventId,
                notification.HostUserId,
                notification.TenantId,
                NotificationType.CalendarSync,
                executeAt: now.AddSeconds(3),
                cancellationToken);

            _logger.LogDebug("Queued CalendarSync for booking {BookingId}", booking.Id);

            // 3. Queue all email notifications (confirmation, reminder, follow-up)
            // These run after video and calendar tasks complete
            // The service handles scheduling based on event settings
            await _notificationQueueService.QueueBookingNotificationsAsync(
                booking,
                booking.MeetlrEvent,
                NotificationTrigger.BookingCreated,
                cancellationToken);

            _logger.LogInformation(
                "Successfully queued all tasks for booking {BookingId}. " +
                "VideoRequired: {VideoRequired}, MeetingLink: {MeetingLink}",
                notification.BookingId,
                IsVideoLocationRequired(booking.MeetlrEvent.MeetingLocationType),
                booking.MeetingLink ?? "pending");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to queue tasks for booking {BookingId}",
                notification.BookingId);
            // Don't rethrow - task queuing failure shouldn't break the booking flow
        }
    }

    /// <summary>
    /// Checks if the meeting location type requires video conferencing
    /// </summary>
    private static bool IsVideoLocationRequired(MeetingLocationType locationType)
    {
        return locationType switch
        {
            MeetingLocationType.Zoom => true,
            MeetingLocationType.GoogleMeet => true,
            MeetingLocationType.MicrosoftTeams => true,
            MeetingLocationType.JitsiMeet => true,
            _ => false
        };
    }
}
