using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Application.Interfaces.Models;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Application.Features.Bookings.Commands.PauseSeriesBooking;

public class PauseSeriesBookingCommandHandler : IRequestHandler<PauseSeriesBookingCommand, PauseSeriesBookingCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICalendarService? _calendarService;
    private readonly ILogger<PauseSeriesBookingCommandHandler> _logger;

    public PauseSeriesBookingCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ILogger<PauseSeriesBookingCommandHandler> logger,
        ICalendarService? calendarService = null)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _calendarService = calendarService;
        _logger = logger;
    }

    public async Task<PauseSeriesBookingCommandResponse> Handle(
        PauseSeriesBookingCommand request,
        CancellationToken cancellationToken)
    {
        var series = await _unitOfWork.Repository<BookingSeries>()
            .GetByIdAsync(request.SeriesId, cancellationToken);

        if (series == null)
            throw BookingErrors.SeriesNotFound(request.SeriesId);

        var oldStatus = series.Status;

        if (request.Resume)
        {
            if (series.Status != SeriesStatus.Paused)
                throw BookingErrors.SeriesNotPaused(request.SeriesId);

            series.Status = SeriesStatus.Active;
            series.UpdatedAt = DateTime.UtcNow;

            // Recreate future calendar events (include Contact for invitee info)
            var futureBookings = _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .Include(b => b.Contact)
                .Where(b => b.SeriesBookingId == series.Id && !b.IsDeleted && b.StartTime > DateTime.UtcNow)
                .ToList();

            foreach (var booking in futureBookings)
            {
                try
                {
                    var eventType = await _unitOfWork.Repository<MeetlrEvent>()
                        .GetByIdAsync(series.BaseMeetlrEventId, cancellationToken);

                    if (eventType != null && _calendarService != null)
                    {
                        var inviteeName = booking.Contact?.Name ?? "Guest";
                        var inviteeEmail = booking.Contact?.Email ?? string.Empty;

                        var result = await _calendarService.CreateEventAsync(
                            series.HostUserId,
                            new CalendarServiceEventRequest
                            {
                                BookingId = booking.Id,
                                Summary = $"{eventType.Name} with {inviteeName}",
                                Description = $"Resumed recurring booking\n\nInvitee: {inviteeName}\nEmail: {inviteeEmail}",
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
                    _logger.LogError(ex, "Failed to recreate calendar event for booking {BookingId}", booking.Id);
                }
            }
        }
        else
        {
            if (series.Status != SeriesStatus.Active)
                throw BookingErrors.SeriesNotActive(request.SeriesId);

            series.Status = SeriesStatus.Paused;
            series.UpdatedAt = DateTime.UtcNow;

            // Delete future calendar events
            var futureBookings = _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .Where(b => b.SeriesBookingId == series.Id && !b.IsDeleted && b.StartTime > DateTime.UtcNow)
                .ToList();

            foreach (var booking in futureBookings)
            {
                try
                {
                    if (!string.IsNullOrEmpty(booking.CalendarEventId) && _calendarService != null)
                    {
                        await _calendarService.DeleteEventAsync(
                            series.HostUserId,
                            booking.CalendarEventId,
                            cancellationToken);
                        booking.CalendarEventId = null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete calendar event for booking {BookingId}", booking.Id);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Log audit
        await _auditService.LogAsync(
            AuditEntityType.BookingSeries,
            series.Id.ToString(),
            AuditAction.Pause,
            oldStatus,
            series.Status,
            cancellationToken
        );

        return new PauseSeriesBookingCommandResponse
        {
            Success = true,
            Message = request.Resume ? "Series resumed successfully" : "Series paused successfully",
            NewStatus = series.Status.ToString()
        };
    }
}
