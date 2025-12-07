using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.Extensions.Logging;

namespace Meetlr.Application.Features.Bookings.Commands.CancelSeriesBooking;

public class CancelSeriesBookingCommandHandler : IRequestHandler<CancelSeriesBookingCommand, CancelSeriesBookingCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationQueueService _notificationQueueService;
    private readonly IAuditService _auditService;
    private readonly ICalendarService? _calendarService;
    private readonly ILogger<CancelSeriesBookingCommandHandler> _logger;

    public CancelSeriesBookingCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationQueueService notificationQueueService,
        IAuditService auditService,
        ILogger<CancelSeriesBookingCommandHandler> logger,
        ICalendarService? calendarService = null)
    {
        _unitOfWork = unitOfWork;
        _notificationQueueService = notificationQueueService;
        _auditService = auditService;
        _calendarService = calendarService;
        _logger = logger;
    }

    public async Task<CancelSeriesBookingCommandResponse> Handle(
        CancelSeriesBookingCommand request,
        CancellationToken cancellationToken)
    {
        // Get series with bookings
        var series = await _unitOfWork.Repository<BookingSeries>()
            .GetByIdAsync(request.SeriesId, cancellationToken);

        if (series == null)
            throw BookingErrors.SeriesNotFound(request.SeriesId);

        // Get event type for payment info
        var eventType = await _unitOfWork.Repository<MeetlrEvent>()
            .GetByIdAsync(series.BaseMeetlrEventId, cancellationToken);

        if (eventType == null)
            throw MeetlrEventErrors.MeetlrEventNotFound(series.BaseMeetlrEventId);

        // Get all bookings for this series
        var allBookings = _unitOfWork.Repository<Booking>()
            .GetQueryable()
            .Where(b => b.SeriesBookingId == series.Id && !b.IsDeleted)
            .ToList();

        // Determine which bookings to cancel based on scope
        var bookingsToCancel = request.Scope switch
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

        int cancelledCount = 0;
        decimal refundAmount = 0;

        foreach (var booking in bookingsToCancel)
        {
            // Soft delete
            booking.IsDeleted = true;
            booking.DeletedAt = DateTime.UtcNow;
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationReason = request.Reason;
            booking.CancelledAt = DateTime.UtcNow;

            // Delete calendar events
            try
            {
                if (!string.IsNullOrEmpty(booking.CalendarEventId) && _calendarService != null)
                {
                    await _calendarService.DeleteEventAsync(
                        series.HostUserId,
                        booking.CalendarEventId,
                        cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete calendar event for booking {BookingId}", booking.Id);
            }

            // Track refund amount for completed payments
            if (booking.Amount.HasValue && booking.PaymentStatus == PaymentStatus.Completed)
            {
                refundAmount += booking.Amount.Value;
            }

            cancelledCount++;
        }

        // Update series status
        if (request.Scope == CancellationScope.AllOccurrences)
        {
            series.Status = SeriesStatus.Cancelled;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Queue refunds for each booking with completed payment - background service will process with retries
        int refundsQueued = 0;
        foreach (var booking in bookingsToCancel.Where(b =>
            b.PaymentStatus == PaymentStatus.Completed &&
            !string.IsNullOrEmpty(b.PaymentIntentId)))
        {
            _logger.LogInformation(
                "Queueing refund for booking {BookingId}, PaymentIntentId: {PaymentIntentId}, Amount: {Amount}",
                booking.Id, booking.PaymentIntentId, booking.Amount);

            // Mark as refund pending - background service will update to Refunded on success
            booking.PaymentStatus = PaymentStatus.RefundPending;

            // Queue refund for background processing with retry logic
            await _notificationQueueService.QueueRefundAsync(booking, cancellationToken);
            refundsQueued++;
        }

        if (refundsQueued > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Queued {Count} refunds for series {SeriesId}", refundsQueued, series.Id);
        }

        // Queue cancellation notifications
        foreach (var booking in bookingsToCancel.Take(1)) // Send one notification for the series
        {
            await _notificationQueueService.QueueBookingNotificationsAsync(
                booking,
                eventType,
                NotificationTrigger.BookingCancelled,
                cancellationToken
            );
        }

        // Log audit
        await _auditService.LogAsync(
            AuditEntityType.BookingSeries,
            series.Id.ToString(),
            AuditAction.Cancel,
            series,
            series,
            cancellationToken
        );

        return new CancelSeriesBookingCommandResponse
        {
            Success = true,
            Message = refundsQueued > 0
                ? $"Successfully cancelled {cancelledCount} booking(s). {refundsQueued} refund(s) queued for processing."
                : $"Successfully cancelled {cancelledCount} booking(s)",
            BookingsCancelled = cancelledCount,
            RefundAmount = refundAmount > 0 ? refundAmount : null
        };
    }
}
