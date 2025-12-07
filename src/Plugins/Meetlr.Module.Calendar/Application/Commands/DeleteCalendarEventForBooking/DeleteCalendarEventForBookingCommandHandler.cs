using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Entities.Notifications;
using Meetlr.Module.Calendar.Application.Commands.DeleteCalendarEvent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Module.Calendar.Application.Commands.DeleteCalendarEventForBooking;

/// <summary>
/// Handler responsible ONLY for deleting calendar events for bookings.
/// Does NOT manage pending/history tables - that's the background service's responsibility.
/// </summary>
public class DeleteCalendarEventForBookingCommandHandler : IRequestHandler<DeleteCalendarEventForBookingCommand, DeleteCalendarEventForBookingCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<DeleteCalendarEventForBookingCommandHandler> _logger;

    public DeleteCalendarEventForBookingCommandHandler(
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger<DeleteCalendarEventForBookingCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<DeleteCalendarEventForBookingCommandResponse> Handle(
        DeleteCalendarEventForBookingCommand request,
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
                    "Notification {NotificationId} not found for calendar event deletion",
                    request.NotificationPendingId);

                return new DeleteCalendarEventForBookingCommandResponse
                {
                    Success = false,
                    ErrorMessage = "Notification not found"
                };
            }

            // 2. Get Booking with MeetlrEvent (need scheduleId)
            var booking = await _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .Include(b => b.MeetlrEvent)
                .FirstOrDefaultAsync(b => b.Id == notification.BookingId, cancellationToken);

            if (booking == null)
            {
                _logger.LogWarning(
                    "Booking {BookingId} not found for calendar event deletion",
                    notification.BookingId);

                return new DeleteCalendarEventForBookingCommandResponse
                {
                    Success = false,
                    ErrorMessage = "Booking not found"
                };
            }

            // 3. Check if booking has calendar event to delete
            if (string.IsNullOrEmpty(booking.CalendarEventId))
            {
                _logger.LogDebug(
                    "Booking {BookingId} has no calendar event ID, skipping deletion",
                    booking.Id);

                return new DeleteCalendarEventForBookingCommandResponse { Success = true };
            }

            _logger.LogInformation(
                "Deleting calendar event for booking {BookingId}, CalendarEventId: {CalendarEventId}",
                booking.Id, booking.CalendarEventId);

            // 4. Delete calendar event via existing command
            var scheduleId = booking.MeetlrEvent.AvailabilityScheduleId;
            var deleteCalendarEventCommand = new DeleteCalendarEventCommand
            {
                ScheduleId = scheduleId,
                CalendarEventId = booking.CalendarEventId
            };

            var deleteResult = await _mediator.Send(deleteCalendarEventCommand, cancellationToken);

            if (deleteResult.Success)
            {
                // 5. Clear calendar event ID from booking
                booking.CalendarEventId = null;
                booking.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<Booking>().Update(booking);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Calendar event deleted for booking {BookingId}",
                    booking.Id);

                return new DeleteCalendarEventForBookingCommandResponse { Success = true };
            }
            else
            {
                // Handle failure
                var errorMessage = deleteResult.Results?.FirstOrDefault(r => !r.Success)?.Error ?? "Unknown error";
                _logger.LogWarning(
                    "Calendar event deletion failed for booking {BookingId}: {Error}",
                    booking.Id, errorMessage);

                return new DeleteCalendarEventForBookingCommandResponse
                {
                    Success = false,
                    ErrorMessage = errorMessage
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Exception while deleting calendar event for notification {NotificationId}",
                request.NotificationPendingId);

            return new DeleteCalendarEventForBookingCommandResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
