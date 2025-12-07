using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Entities.Notifications;
using Meetlr.Domain.Entities.Scheduling;
using Meetlr.Module.Calendar.Application.Commands.CreateCalendarEvent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CalendarIntegration = Meetlr.Module.Calendar.Domain.Entities.CalendarIntegration;
using Entities_CalendarIntegration = Meetlr.Module.Calendar.Domain.Entities.CalendarIntegration;

namespace Meetlr.Module.Calendar.Application.Commands.CreateCalendarEventForBooking;

/// <summary>
/// Handler responsible ONLY for creating calendar events for bookings.
/// Does NOT manage pending/history tables - that's the background service's responsibility.
/// </summary>
public class CreateCalendarEventForBookingCommandHandler : IRequestHandler<CreateCalendarEventForBookingCommand, CreateCalendarEventForBookingCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<CreateCalendarEventForBookingCommandHandler> _logger;

    public CreateCalendarEventForBookingCommandHandler(
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger<CreateCalendarEventForBookingCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<CreateCalendarEventForBookingCommandResponse> Handle(
        CreateCalendarEventForBookingCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. Get NotificationPending by ID
            var notification = await _unitOfWork.Repository<NotificationPending>()
                .GetByIdAsync(request.NotificationPendingId, cancellationToken);

            if (notification == null)
            {
                _logger.LogWarning(
                    "Notification {NotificationId} not found for calendar event creation",
                    request.NotificationPendingId);

                return new CreateCalendarEventForBookingCommandResponse
                {
                    Success = false,
                    ErrorMessage = "Notification not found"
                };
            }

            // 2. Get Booking with MeetlrEvent first (need scheduleId)
            var booking = await _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .Include(b => b.MeetlrEvent)
                    .ThenInclude(e => e.User)
                .Include(b => b.Contact)
                .FirstOrDefaultAsync(b => b.Id == notification.BookingId, cancellationToken);

            if (booking == null)
            {
                _logger.LogWarning(
                    "Booking {BookingId} not found for calendar event creation",
                    notification.BookingId);

                return new CreateCalendarEventForBookingCommandResponse
                {
                    Success = false,
                    ErrorMessage = "Booking not found"
                };
            }

            // 3. Check if schedule has auto-sync enabled calendar
            var scheduleId = booking.MeetlrEvent.AvailabilityScheduleId;
            var hasAutoSync = await _unitOfWork.Repository<Entities_CalendarIntegration>()
                .GetQueryable()
                .AnyAsync(ci =>
                    ci.AvailabilityScheduleId == scheduleId &&
                    ci.IsActive &&
                    ci.IsPrimaryCalendar &&
                    ci.AddEventsToCalendar,
                    cancellationToken);

            if (!hasAutoSync)
            {
                _logger.LogDebug(
                    "No auto-sync calendar configured for schedule {ScheduleId}, skipping calendar event creation",
                    scheduleId);

                return new CreateCalendarEventForBookingCommandResponse { Success = true };
            }

            // 4. Check if booking already has calendar event (idempotency)
            if (!string.IsNullOrEmpty(booking.CalendarEventId))
            {
                _logger.LogDebug(
                    "Booking {BookingId} already has calendar event {CalendarEventId}, skipping",
                    booking.Id, booking.CalendarEventId);

                return new CreateCalendarEventForBookingCommandResponse
                {
                    Success = true,
                    CalendarEventId = booking.CalendarEventId
                };
            }

            _logger.LogInformation(
                "Creating calendar event for booking {BookingId}",
                booking.Id);

            // 5. Build calendar event description
            var description = $"Booking confirmed via Meetlr\n\n" +
                              $"(Event: {booking.MeetlrEvent.Name})\n\n" +
                              $"Invitee: {booking.Contact?.Name}\n" +
                              $"Email: {booking.Contact?.Email}\n";

            if (!string.IsNullOrEmpty(booking.Notes))
            {
                description += $"\nNotes: {booking.Notes}";
            }

            // 6. Create calendar event via existing command
            var createCalendarEventCommand = new CreateCalendarEventCommand
            {
                ScheduleId = scheduleId,
                Summary = $"{booking.MeetlrEvent.Name} with {booking.Contact?.Name}",
                Description = description,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                TimeZone = booking.MeetlrEvent.User?.TimeZone ?? "UTC",
                AttendeeEmails = booking.Contact?.Email != null
                    ? new List<string> { booking.Contact.Email }
                    : new List<string>(),
                Location = booking.Location,
                MeetingLink = booking.MeetingLink,
                BufferBeforeMinutes = booking.MeetlrEvent.BufferTimeBeforeMinutes,
                BufferAfterMinutes = booking.MeetlrEvent.BufferTimeAfterMinutes
            };

            var calendarResult = await _mediator.Send(createCalendarEventCommand, cancellationToken);

            if (calendarResult.Success && calendarResult.Results?.Any() == true)
            {
                var eventIds = calendarResult.Results
                    .Where(r => r.Success && !string.IsNullOrEmpty(r.EventId))
                    .ToDictionary(r => r.Provider, r => r.EventId!);

                if (eventIds.Any())
                {
                    // 7. Update booking with calendar event ID
                    booking.CalendarEventId = System.Text.Json.JsonSerializer.Serialize(eventIds);
                    booking.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Repository<Booking>().Update(booking);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation(
                        "Calendar event created for booking {BookingId}: {EventIds}",
                        booking.Id, booking.CalendarEventId);

                    return new CreateCalendarEventForBookingCommandResponse
                    {
                        Success = true,
                        CalendarEventId = booking.CalendarEventId
                    };
                }
            }

            // Handle failure - no successful calendar events created
            var errorMessage = calendarResult.Results?.FirstOrDefault(r => !r.Success)?.Error ?? "Unknown error";
            _logger.LogWarning(
                "Calendar event creation failed for booking {BookingId}: {Error}",
                booking.Id, errorMessage);

            return new CreateCalendarEventForBookingCommandResponse
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Exception while creating calendar event for notification {NotificationId}",
                request.NotificationPendingId);

            return new CreateCalendarEventForBookingCommandResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
