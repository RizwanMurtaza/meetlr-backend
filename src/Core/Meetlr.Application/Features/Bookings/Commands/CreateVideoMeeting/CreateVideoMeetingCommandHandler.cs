using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Plugins.MeetingTypes;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Application.Features.Bookings.Commands.CreateVideoMeeting;

/// <summary>
/// Handler responsible ONLY for creating video meetings.
/// Does NOT manage pending/history tables - that's the background service's responsibility.
/// </summary>
public class CreateVideoMeetingCommandHandler : IRequestHandler<CreateVideoMeetingCommand, CreateVideoMeetingCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMeetingTypeService? _meetingTypeService;
    private readonly ILogger<CreateVideoMeetingCommandHandler> _logger;

    public CreateVideoMeetingCommandHandler(
        IUnitOfWork unitOfWork,
        IMeetingTypeService? meetingTypeService,
        ILogger<CreateVideoMeetingCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _meetingTypeService = meetingTypeService;
        _logger = logger;
    }

    public async Task<CreateVideoMeetingCommandResponse> Handle(
        CreateVideoMeetingCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var notification = await _unitOfWork.Repository<NotificationPending>()
                .GetByIdAsync(request.NotificationPendingId, cancellationToken);

            if (notification == null)
            {
                _logger.LogWarning(
                    "Notification {NotificationId} not found for video meeting creation",
                    request.NotificationPendingId);

                return new CreateVideoMeetingCommandResponse
                {
                    Success = false,
                    ErrorMessage = "Notification not found"
                };
            }

            // Check if meeting type service is available
            if (_meetingTypeService == null)
            {
                _logger.LogDebug("Meeting type service not available, skipping");
                return new CreateVideoMeetingCommandResponse { Success = true };
            }

            // Get Booking with MeetlrEvent and Contact
            var booking = await _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .Include(b => b.MeetlrEvent)
                    .ThenInclude(e => e.User)
                .Include(b => b.Contact)
                .FirstOrDefaultAsync(b => b.Id == notification.BookingId, cancellationToken);

            if (booking == null)
            {
                _logger.LogWarning(
                    "Booking {BookingId} not found for video meeting creation",
                    notification.BookingId);

                return new CreateVideoMeetingCommandResponse
                {
                    Success = false,
                    ErrorMessage = "Booking not found"
                };
            }

            // Check if booking already has a meeting link (idempotency)
            if (!string.IsNullOrEmpty(booking.MeetingLink))
            {
                _logger.LogDebug(
                    "Booking {BookingId} already has meeting link, skipping creation",
                    booking.Id);

                return new CreateVideoMeetingCommandResponse
                {
                    Success = true,
                    MeetingLink = booking.MeetingLink,
                    MeetingId = booking.MeetingId
                };
            }

            // Check if event's location type requires video conferencing
            if (!_meetingTypeService.IsVideoLocationType(booking.MeetlrEvent.MeetingLocationType))
            {
                _logger.LogDebug(
                    "Booking {BookingId} location type {LocationType} does not require video conferencing",
                    booking.Id, booking.MeetlrEvent.MeetingLocationType);

                return new CreateVideoMeetingCommandResponse { Success = true };
            }

            _logger.LogInformation(
                "Creating video meeting for booking {BookingId}, LocationType: {LocationType}",
                booking.Id, booking.MeetlrEvent.MeetingLocationType);

            // Create video meeting
            var meetingResult = await _meetingTypeService.CreateMeetingAsync(
                booking.MeetlrEvent.MeetingLocationType,
                new MeetingCreationRequest
                {
                    Title = $"{booking.MeetlrEvent.Name} with {booking.Contact?.Name ?? "Guest"}",
                    StartTime = booking.StartTime,
                    DurationMinutes = booking.MeetlrEvent.DurationMinutes,
                    UserId = booking.MeetlrEvent.UserId,
                    EventSlug = booking.MeetlrEvent.Slug,
                    BookingId = booking.Id,
                    AttendeeEmail = booking.Contact?.Email,
                    AttendeeName = booking.Contact?.Name
                },
                cancellationToken);

            if (meetingResult != null)
            {
                // Update booking with meeting link
                booking.MeetingLink = meetingResult.JoinUrl;
                booking.MeetingId = meetingResult.MeetingId;
                booking.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<Booking>().Update(booking);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Video meeting created for booking {BookingId}. MeetingId: {MeetingId}",
                    booking.Id, meetingResult.MeetingId);

                return new CreateVideoMeetingCommandResponse
                {
                    Success = true,
                    MeetingLink = meetingResult.JoinUrl,
                    MeetingId = meetingResult.MeetingId
                };
            }
            else
            {
                _logger.LogWarning(
                    "Video meeting creation returned null for booking {BookingId}",
                    booking.Id);

                return new CreateVideoMeetingCommandResponse
                {
                    Success = false,
                    ErrorMessage = "Video meeting creation returned null"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Exception while creating video meeting for notification {NotificationId}",
                request.NotificationPendingId);

            return new CreateVideoMeetingCommandResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
