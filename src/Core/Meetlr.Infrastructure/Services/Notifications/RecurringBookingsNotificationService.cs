using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Meetlr.Infrastructure.Services.Notifications;

/// <summary>
/// Handles queuing notifications for booking series
/// Responsibility: Queue ONE series confirmation, individual reminders, and ONE follow-up
/// </summary>
public class RecurringBookingsNotificationService
{
    private readonly NotificationChannelDispatcher _channelDispatcher;
    private readonly ILogger<RecurringBookingsNotificationService> _logger;

    public RecurringBookingsNotificationService(
        NotificationChannelDispatcher channelDispatcher,
        ILogger<RecurringBookingsNotificationService> logger)
    {
        _channelDispatcher = channelDispatcher;
        _logger = logger;
    }

    /// <summary>
    /// Queue all notifications for a booking series
    /// - ONE series confirmation (immediate)
    /// - Individual reminders per booking (scheduled before each booking)
    /// - ONE follow-up after last booking
    /// </summary>
    public async Task<int> QueueSeriesNotificationsAsync(
        BookingSeries series,
        List<Booking> bookings,
        MeetlrEvent meetlrEvent,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Queuing notifications for booking series {SeriesId} with {Count} bookings",
            series.Id,
            bookings.Count);

        if (bookings.Count == 0)
        {
            _logger.LogWarning("No bookings provided for series {SeriesId}, skipping notification queueing", series.Id);
            return 0;
        }

        var firstBooking = bookings.OrderBy(b => b.StartTime).First();
        var notificationsQueued = 0;

        // 1. Queue ONE series confirmation (sent immediately)
        notificationsQueued += await QueueSeriesConfirmationAsync(series, firstBooking, meetlrEvent, cancellationToken);

        // 2. Queue individual reminders for each booking (scheduled before each booking)
        notificationsQueued += await QueueIndividualRemindersAsync(bookings, meetlrEvent, cancellationToken);

        // 3. Queue ONE follow-up for the entire series (after last booking)
        notificationsQueued += await QueueSeriesFollowUpAsync(bookings, meetlrEvent, cancellationToken);

        _logger.LogInformation(
            "Queued {Count} notifications for booking series {SeriesId}",
            notificationsQueued,
            series.Id);

        return notificationsQueued;
    }

    /// <summary>
    /// Queue ONE series confirmation notification (sent immediately)
    /// </summary>
    private async Task<int> QueueSeriesConfirmationAsync(
        BookingSeries series,
        Booking firstBooking,
        MeetlrEvent meetlrEvent,
        CancellationToken cancellationToken)
    {
        var scheduledAt = DateTime.UtcNow;
        var executeAt = DateTime.UtcNow;

        return await _channelDispatcher.QueueToEnabledChannelsAsync(
            firstBooking.Id, // Use first booking ID for series confirmation
            meetlrEvent.Id,
            meetlrEvent.UserId,
            NotificationTrigger.SeriesCreated,
            meetlrEvent,
            series.Contact?.Email ?? string.Empty,
            series.Contact?.Phone,
            scheduledAt,
            executeAt,
            cancellationToken);
    }

    /// <summary>
    /// Queue individual reminder notifications for each booking (scheduled before each booking)
    /// </summary>
    private async Task<int> QueueIndividualRemindersAsync(
        List<Booking> bookings,
        MeetlrEvent meetlrEvent,
        CancellationToken cancellationToken)
    {
        if (!meetlrEvent.SendReminderEmail || meetlrEvent.ReminderHoursBefore <= 0)
            return 0;

        var notificationsQueued = 0;

        foreach (var booking in bookings)
        {
            var reminderScheduledAt = booking.StartTime.AddHours(-meetlrEvent.ReminderHoursBefore);

            // Only queue if reminder time is in the future
            if (reminderScheduledAt > DateTime.UtcNow)
            {
                notificationsQueued += await _channelDispatcher.QueueToEnabledChannelsAsync(
                    booking.Id,
                    meetlrEvent.Id,
                    meetlrEvent.UserId,
                    NotificationTrigger.BookingReminder,
                    meetlrEvent,
                    booking.Contact?.Email ?? string.Empty,
                    booking.Contact?.Phone,
                    reminderScheduledAt,
                    reminderScheduledAt,
                    cancellationToken);
            }
        }

        return notificationsQueued;
    }

    /// <summary>
    /// Queue ONE follow-up notification for the entire series (after last booking)
    /// </summary>
    private async Task<int> QueueSeriesFollowUpAsync(
        List<Booking> bookings,
        MeetlrEvent meetlrEvent,
        CancellationToken cancellationToken)
    {
        if (!meetlrEvent.SendFollowUpEmail || meetlrEvent.FollowUpHoursAfter <= 0)
            return 0;

        var lastBooking = bookings.OrderByDescending(b => b.EndTime).First();
        var followUpScheduledAt = lastBooking.EndTime.AddHours(meetlrEvent.FollowUpHoursAfter);

        return await _channelDispatcher.QueueToEnabledChannelsAsync(
            lastBooking.Id,
            meetlrEvent.Id,
            meetlrEvent.UserId,
            NotificationTrigger.BookingFollowUp,
            meetlrEvent,
            lastBooking.Contact?.Email ?? string.Empty,
            lastBooking.Contact?.Phone,
            followUpScheduledAt,
            followUpScheduledAt,
            cancellationToken);
    }
}
