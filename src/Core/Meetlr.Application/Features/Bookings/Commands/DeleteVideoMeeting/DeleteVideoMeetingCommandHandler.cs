using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Plugins.MeetingTypes;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Application.Features.Bookings.Commands.DeleteVideoMeeting;

/// <summary>
/// Handler responsible ONLY for deleting video meetings.
/// Does NOT manage pending/history tables - that's the background service's responsibility.
/// </summary>
public class DeleteVideoMeetingCommandHandler : IRequestHandler<DeleteVideoMeetingCommand, DeleteVideoMeetingCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMeetingTypeService? _meetingTypeService;
    private readonly ILogger<DeleteVideoMeetingCommandHandler> _logger;

    public DeleteVideoMeetingCommandHandler(
        IUnitOfWork unitOfWork,
        IMeetingTypeService? meetingTypeService,
        ILogger<DeleteVideoMeetingCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _meetingTypeService = meetingTypeService;
        _logger = logger;
    }

    public async Task<DeleteVideoMeetingCommandResponse> Handle(
        DeleteVideoMeetingCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var notification = await _unitOfWork.Repository<NotificationPending>()
                .GetByIdAsync(request.NotificationPendingId, cancellationToken);

            if (notification == null)
            {
                _logger.LogWarning(
                    "Notification {NotificationId} not found for video meeting deletion",
                    request.NotificationPendingId);

                return new DeleteVideoMeetingCommandResponse
                {
                    Success = false,
                    ErrorMessage = "Notification not found"
                };
            }

            // Check if meeting type service is available
            if (_meetingTypeService == null)
            {
                _logger.LogDebug("Meeting type service not available, skipping deletion");
                return new DeleteVideoMeetingCommandResponse { Success = true };
            }

            // Get Booking with MeetlrEvent
            var booking = await _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .Include(b => b.MeetlrEvent)
                .FirstOrDefaultAsync(b => b.Id == notification.BookingId, cancellationToken);

            if (booking == null)
            {
                _logger.LogWarning(
                    "Booking {BookingId} not found for video meeting deletion",
                    notification.BookingId);

                return new DeleteVideoMeetingCommandResponse
                {
                    Success = false,
                    ErrorMessage = "Booking not found"
                };
            }

            // Check if booking has a meeting ID to delete
            if (string.IsNullOrEmpty(booking.MeetingId))
            {
                _logger.LogDebug(
                    "Booking {BookingId} has no meeting ID, skipping deletion",
                    booking.Id);

                return new DeleteVideoMeetingCommandResponse { Success = true };
            }

            // Check if event's location type requires video conferencing
            if (!_meetingTypeService.IsVideoLocationType(booking.MeetlrEvent.MeetingLocationType))
            {
                _logger.LogDebug(
                    "Booking {BookingId} location type {LocationType} does not require video conferencing",
                    booking.Id, booking.MeetlrEvent.MeetingLocationType);

                return new DeleteVideoMeetingCommandResponse { Success = true };
            }

            _logger.LogInformation(
                "Deleting video meeting for booking {BookingId}, MeetingId: {MeetingId}",
                booking.Id, booking.MeetingId);

            // Delete video meeting
            var success = await _meetingTypeService.DeleteMeetingAsync(
                booking.MeetlrEvent.MeetingLocationType,
                booking.MeetingId,
                booking.MeetlrEvent.UserId,
                cancellationToken);

            if (success)
            {
                // Clear meeting ID from booking
                booking.MeetingId = null;
                booking.MeetingLink = null;
                booking.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<Booking>().Update(booking);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Video meeting deleted for booking {BookingId}",
                    booking.Id);

                return new DeleteVideoMeetingCommandResponse { Success = true };
            }
            else
            {
                _logger.LogWarning(
                    "Video meeting deletion failed for booking {BookingId}",
                    booking.Id);

                return new DeleteVideoMeetingCommandResponse
                {
                    Success = false,
                    ErrorMessage = "Video meeting deletion failed"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Exception while deleting video meeting for notification {NotificationId}",
                request.NotificationPendingId);

            return new DeleteVideoMeetingCommandResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
