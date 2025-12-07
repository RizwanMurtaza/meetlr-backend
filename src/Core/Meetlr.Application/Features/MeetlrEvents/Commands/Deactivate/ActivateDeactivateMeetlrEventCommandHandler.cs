using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Features.Bookings.Commands.CancelBooking;
using Meetlr.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Application.Features.MeetlrEvents.Commands.Deactivate;

public class ActivateDeactivateMeetlrEventCommandHandler : IRequestHandler<ActivateDeactivateMeetlrEventCommand, ActivateDeactivateMeetlrEventCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<ActivateDeactivateMeetlrEventCommandHandler> _logger;

    public ActivateDeactivateMeetlrEventCommandHandler(
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger<ActivateDeactivateMeetlrEventCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<ActivateDeactivateMeetlrEventCommandResponse> Handle(ActivateDeactivateMeetlrEventCommand request, CancellationToken cancellationToken)
    {
        var eventType = await _unitOfWork.Repository<Domain.Entities.Events.MeetlrEvent>().GetQueryable()
            .Include(e => e.Bookings)
            .FirstOrDefaultAsync(e => e.Id == request.MeetlrEventId && e.UserId == request.UserId, cancellationToken);

        if (eventType == null)
        {
            return new ActivateDeactivateMeetlrEventCommandResponse
            {
                Success = false,
                Message = "Event type not found"
            };
        }

        // Update the event type status
        eventType.IsActive = request.IsActive;
        eventType.Status = request.IsActive ? MeetlrEventStatus.Active : MeetlrEventStatus.Inactive;
        _unitOfWork.Repository<Domain.Entities.Events.MeetlrEvent>().Update(eventType);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        int cancelledCount = 0;

        // Cancel future bookings only when deactivating and if requested
        // This ensures all booking cancellation logic is executed:
        // - Calendar event deletion (Google/Microsoft)
        // - Email notifications to invitees
        // - Refund processing for paid events
        // - Audit logging
        if (!request.IsActive && request.CancelFutureBookings)
        {
            var futureBookings = eventType.Bookings
                .Where(b => b.Status == BookingStatus.Confirmed && b.StartTime > DateTime.UtcNow)
                .ToList();

            foreach (var booking in futureBookings)
            {
                try
                {
                    var cancelCommand = new CancelBookingCommand
                    {
                        BookingId = booking.Id,
                        CancellationReason = request.CancellationReason ?? "Event type has been deactivated"
                    };

                    await _mediator.Send(cancelCommand, cancellationToken);
                    cancelledCount++;
                }
                catch (Exception ex)
                {
                    // Log but continue with other bookings
                    _logger.LogError(ex, "Failed to cancel booking {BookingId}", booking.Id);
                }
            }
        }

        // Build appropriate response message
        string message;
        if (request.IsActive)
        {
            message = "Event type activated successfully";
        }
        else
        {
            message = cancelledCount > 0
                ? $"Event type deactivated and {cancelledCount} future booking(s) cancelled"
                : "Event type deactivated successfully";
        }

        return new ActivateDeactivateMeetlrEventCommandResponse
        {
            Success = true,
            IsActive = request.IsActive,
            CancelledBookingsCount = cancelledCount,
            Message = message
        };
    }
}
