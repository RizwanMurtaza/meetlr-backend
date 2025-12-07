using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Application.Interfaces.Models;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Application.Features.Bookings.Commands.UpdateSeriesBooking;

public class UpdateSeriesBookingCommandHandler : IRequestHandler<UpdateSeriesBookingCommand, UpdateSeriesBookingCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationQueueService _notificationQueueService;
    private readonly IAuditService _auditService;
    private readonly ICalendarService? _calendarService;
    private readonly ILogger<UpdateSeriesBookingCommandHandler> _logger;

    public UpdateSeriesBookingCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationQueueService notificationQueueService,
        IAuditService auditService,
        ILogger<UpdateSeriesBookingCommandHandler> logger,
        ICalendarService? calendarService = null)
    {
        _unitOfWork = unitOfWork;
        _notificationQueueService = notificationQueueService;
        _auditService = auditService;
        _calendarService = calendarService;
        _logger = logger;
    }

    public async Task<UpdateSeriesBookingCommandResponse> Handle(
        UpdateSeriesBookingCommand request,
        CancellationToken cancellationToken)
    {
        // Get series
        var series = await _unitOfWork.Repository<BookingSeries>()
            .GetByIdAsync(request.SeriesId, cancellationToken);

        if (series == null)
            throw BookingErrors.SeriesNotFound(request.SeriesId);

        // Get event type
        var eventType = await _unitOfWork.Repository<MeetlrEvent>()
            .GetByIdAsync(series.BaseMeetlrEventId, cancellationToken);

        if (eventType == null)
            throw MeetlrEventErrors.MeetlrEventNotFound(series.BaseMeetlrEventId);

        // Get bookings to update (include Contact for invitee info)
        var allBookings = _unitOfWork.Repository<Booking>()
            .GetQueryable()
            .Include(b => b.Contact)
            .Where(b => b.SeriesBookingId == series.Id && !b.IsDeleted)
            .OrderBy(b => b.StartTime)
            .ToList();

        var bookingsToUpdate = request.Scope switch
        {
            CancellationScope.ThisOccurrence => allBookings
                .Where(b => b.Id == request.StartFromBookingId)
                .ToList(),
            CancellationScope.ThisAndFuture => allBookings
                .Where(b => b.StartTime >= allBookings.First(x => x.Id == request.StartFromBookingId).StartTime)
                .ToList(),
            CancellationScope.AllOccurrences => allBookings,
            _ => allBookings
        };

        var conflicts = new List<ConflictInfo>();
        int updatedCount = 0;

        foreach (var booking in bookingsToUpdate)
        {
            var oldStartTime = booking.StartTime;
            var oldEndTime = booking.EndTime;

            // Update times if provided
            if (request.NewStartTime.HasValue)
            {
                throw BookingErrors.SeriesTimeUpdateNotSupported();
            }

            if (request.NewDuration.HasValue)
            {
                booking.EndTime = booking.StartTime.AddMinutes(request.NewDuration.Value);
            }
            else
            {
                booking.EndTime = booking.StartTime.AddMinutes((oldEndTime - oldStartTime).TotalMinutes);
            }

            // Check for conflicts using calendar service
            if (_calendarService != null)
            {
                try
                {
                    var busyTimes = await _calendarService.GetBusyTimesAsync(
                        series.HostUserId,
                        booking.StartTime,
                        booking.EndTime,
                        cancellationToken);

                    if (busyTimes.Any())
                    {
                        conflicts.Add(new ConflictInfo
                        {
                            BookingId = booking.Id,
                            RequestedTime = booking.StartTime,
                            Reason = "Time slot conflicts with existing calendar event"
                        });

                        // Revert changes
                        booking.StartTime = oldStartTime;
                        booking.EndTime = oldEndTime;
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to check conflicts for booking {BookingId}", booking.Id);
                }
            }

            // Update calendar event
            if (_calendarService != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(booking.CalendarEventId))
                    {
                        // Delete old event
                        await _calendarService.DeleteEventAsync(
                            series.HostUserId,
                            booking.CalendarEventId,
                            cancellationToken);

                        // Create new event
                        var inviteeName = booking.Contact?.Name ?? "Guest";
                        var inviteeEmail = booking.Contact?.Email ?? string.Empty;

                        var result = await _calendarService.CreateEventAsync(
                            series.HostUserId,
                            new CalendarServiceEventRequest
                            {
                                BookingId = booking.Id,
                                Summary = $"{eventType.Name} with {inviteeName}",
                                Description = $"Updated recurring booking\n\nInvitee: {inviteeName}\nEmail: {inviteeEmail}",
                                StartTime = booking.StartTime,
                                EndTime = booking.EndTime,
                                TimeZone = eventType.User?.TimeZone ?? "UTC",
                                AttendeeEmails = string.IsNullOrEmpty(inviteeEmail) ? new List<string>() : new List<string> { inviteeEmail },
                                Location = booking.Location,
                                MeetingLink = booking.MeetingLink
                            },
                            cancellationToken);

                        if (result.Success && !string.IsNullOrEmpty(result.EventId))
                        {
                            booking.CalendarEventId = result.EventId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update calendar event for booking {BookingId}", booking.Id);
                }
            }

            booking.UpdatedAt = DateTime.UtcNow;
            updatedCount++;
        }

        series.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Queue notifications for updated bookings
        foreach (var booking in bookingsToUpdate.Take(1))
        {
            await _notificationQueueService.QueueBookingNotificationsAsync(
                booking,
                eventType,
                NotificationTrigger.BookingRescheduled,
                cancellationToken
            );
        }

        // Log audit
        await _auditService.LogAsync(
            AuditEntityType.BookingSeries,
            series.Id.ToString(),
            AuditAction.Update,
            series,
            series,
            cancellationToken
        );

        return new UpdateSeriesBookingCommandResponse
        {
            Success = conflicts.Count == 0,
            Message = conflicts.Any()
                ? $"Updated {updatedCount} booking(s), {conflicts.Count} conflict(s) detected"
                : $"Successfully updated {updatedCount} booking(s)",
            BookingsUpdated = updatedCount,
            Conflicts = conflicts.Any() ? conflicts : null
        };
    }
}
