using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stripe;

namespace Meetlr.Plugins.Payment.Stripe.Features.Commands.VerifySeriesPaymentStatus;

public class VerifySeriesPaymentStatusCommandHandler : IRequestHandler<VerifySeriesPaymentStatusCommand, VerifySeriesPaymentStatusResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VerifySeriesPaymentStatusCommandHandler> _logger;
    private readonly INotificationQueueService _notificationQueueService;
    private readonly IMediator _mediator;

    public VerifySeriesPaymentStatusCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<VerifySeriesPaymentStatusCommandHandler> logger,
        INotificationQueueService notificationQueueService,
        IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _notificationQueueService = notificationQueueService;
        _mediator = mediator;
    }

    public async Task<VerifySeriesPaymentStatusResponse> Handle(VerifySeriesPaymentStatusCommand request, CancellationToken cancellationToken)
    {
        // Get the booking series with all bookings (include Contact for invitee info)
        var series = await _unitOfWork.Repository<BookingSeries>()
            .GetQueryable()
            .Include(s => s.Bookings)
                .ThenInclude(b => b.MeetlrEvent)
                    .ThenInclude(et => et.User)
            .Include(s => s.Bookings)
                .ThenInclude(b => b.Contact)
            .FirstOrDefaultAsync(s => s.Id == request.SeriesId, cancellationToken);

        if (series == null)
        {
            throw new Exception("Booking series not found");
        }

        // Get the first booking to check payment intent
        var firstBooking = series.Bookings.OrderBy(b => b.StartTime).FirstOrDefault();
        if (firstBooking == null)
        {
            throw new Exception("No bookings found in series");
        }

        if (string.IsNullOrEmpty(firstBooking.PaymentIntentId))
        {
            return new VerifySeriesPaymentStatusResponse
            {
                Status = series.Status.ToString(),
                PaymentStatus = PaymentStatus.Pending.ToString(),
                Message = "No payment intent found for this series"
            };
        }

        // Fetch the payment intent from Stripe
        var service = new PaymentIntentService();
        var paymentIntent = await service.GetAsync(firstBooking.PaymentIntentId, cancellationToken: cancellationToken);

        bool statusChanged = false;
        string previousPaymentStatus = firstBooking.PaymentStatus.ToString();

        // Update payment status based on Stripe payment intent
        switch (paymentIntent.Status)
        {
            case "succeeded":
                if (firstBooking.PaymentStatus != PaymentStatus.Completed)
                {
                    // Update all bookings in the series
                    foreach (var booking in series.Bookings)
                    {
                        booking.PaymentStatus = PaymentStatus.Completed;
                        booking.PaidAt = DateTime.UtcNow;
                        booking.Status = BookingStatus.Confirmed;
                        booking.UpdatedAt = DateTime.UtcNow;
                    }

                    // Save booking status changes first
                    statusChanged = true;
                    _logger.LogInformation("Payment verified as succeeded for booking series {SeriesId}", series.Id);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    // Process each booking: create calendar events and queue notifications
                    bool hasCalendarEventUpdates = false;
                    foreach (var booking in series.Bookings)
                    {
                        // Create calendar event
                        var inviteeName = booking.Contact?.Name ?? "Guest";
                        var inviteeEmail = booking.Contact?.Email ?? string.Empty;
                        var description = $"Booking confirmed via Calendly\n\nInvitee: {inviteeName}\nEmail: {inviteeEmail}\n";
                        if (!string.IsNullOrEmpty(booking.Notes))
                        {
                            description += $"\nNotes: {booking.Notes}";
                        }
                       
                    }

                    // Save all calendar event ID updates in a single transaction
                    if (hasCalendarEventUpdates)
                    {
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }
                }
                break;

            case "requires_payment_method":
                // This is normal - payment form hasn't been submitted yet
                _logger.LogInformation("Payment still requires payment method for series {SeriesId}", series.Id);
                break;

            case "processing":
            case "requires_action":
            case "requires_confirmation":
                // Payment is still pending
                _logger.LogInformation("Payment still pending for series {SeriesId}: {Status}", series.Id, paymentIntent.Status);
                break;

            case "canceled":
                if (firstBooking.PaymentStatus != PaymentStatus.Failed)
                {
                    foreach (var booking in series.Bookings)
                    {
                        booking.PaymentStatus = PaymentStatus.Failed;
                        booking.Status = BookingStatus.Cancelled;
                        booking.UpdatedAt = DateTime.UtcNow;
                    }
                    statusChanged = true;
                    _logger.LogWarning("Payment verified as canceled for series {SeriesId}", series.Id);
                }
                break;
        }

        if (statusChanged && firstBooking.PaymentStatus != PaymentStatus.Completed)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Payment status updated for series {SeriesId} from {PreviousStatus} to {NewStatus}",
                series.Id, previousPaymentStatus, firstBooking.PaymentStatus);
        }

        return new VerifySeriesPaymentStatusResponse
        {
            Status = series.Status.ToString(),
            PaymentStatus = firstBooking.PaymentStatus.ToString(),
            Message = statusChanged ? "Payment status updated" : "Payment status unchanged"
        };
    }
}
