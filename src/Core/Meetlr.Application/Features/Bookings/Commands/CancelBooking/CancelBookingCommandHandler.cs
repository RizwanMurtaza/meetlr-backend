using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Application.Features.Bookings.Commands.CancelBooking;

/// <summary>
/// Handler for cancelling bookings.
/// When booking.Status is set to Cancelled, a BookingCancelledEvent is automatically raised.
/// The QueueBookingCancelledTasksHandler then queues all cleanup tasks (video deletion, calendar
/// deletion, refund, notifications) for sequential processing by the background service.
/// </summary>
public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, CancelBookingCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ILogger<CancelBookingCommandHandler> _logger;

    public CancelBookingCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ILogger<CancelBookingCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<CancelBookingCommandResponse> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        // Find booking
        var booking = await _unitOfWork.Repository<Booking>().GetQueryable()
            .Include(b => b.MeetlrEvent)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);

        if (booking == null)
        {
            throw BookingErrors.BookingNotFound(request.BookingId);
        }

        // Verify cancellation token if provided (for public cancellation)
        if (!string.IsNullOrEmpty(request.CancellationToken) &&
            booking.CancellationToken != request.CancellationToken)
        {
            throw BookingErrors.InvalidCancellationToken();
        }

        // Check if booking is already cancelled
        if (booking.Status == BookingStatus.Cancelled)
        {
            throw BookingErrors.BookingAlreadyCancelled();
        }

        // Track if refund will be issued (check before status change)
        var willQueueRefund = booking.PaymentStatus == PaymentStatus.Completed &&
                              !string.IsNullOrEmpty(booking.PaymentIntentId);

        // Update booking status - this auto-raises BookingCancelledEvent via Booking.Status setter
        // The event handler will queue: VideoLinkDeletion, CalendarDeletion, Refund (if applicable), Email notification
        var oldStatus = booking.Status;
        booking.CancellationReason = request.CancellationReason;
        booking.CancelledAt = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;
        booking.Status = BookingStatus.Cancelled; // This raises BookingCancelledEvent

        // Log audit
        await _auditService.LogAsync(
            AuditEntityType.Booking,
            booking.Id.ToString(),
            AuditAction.Cancel,
            new { Status = oldStatus },
            new { Status = booking.Status, CancellationReason = booking.CancellationReason },
            cancellationToken);

        // SaveChanges dispatches domain events which triggers QueueBookingCancelledTasksHandler
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Booking {BookingId} cancelled. BookingCancelledEvent dispatched for cleanup tasks.",
            booking.Id);

        return new CancelBookingCommandResponse
        {
            BookingId = booking.Id,
            Success = true,
            CancelledAt = booking.CancelledAt.Value,
            RefundIssued = willQueueRefund,
            Message = willQueueRefund
                ? "Booking has been cancelled successfully. Refund is being processed."
                : "Booking has been cancelled successfully"
        };
    }
}
