using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

namespace Meetlr.Plugins.Payment.Stripe.Features.Commands.VerifyPaymentStatus;

/// <summary>
/// Verifies payment status from Stripe and raises BookingCompletedEvent on success.
/// Calendar sync and notifications are handled via domain event handlers.
/// </summary>
public class VerifyPaymentStatusCommandHandler : IRequestHandler<VerifyPaymentStatusCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<VerifyPaymentStatusCommandHandler> _logger;

    public VerifyPaymentStatusCommandHandler(
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<VerifyPaymentStatusCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> Handle(VerifyPaymentStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var booking = await _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .Include(b => b.MeetlrEvent)
                    .ThenInclude(et => et.User)
                .Include(b => b.Contact)
                .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);

            if (booking == null)
            {
                _logger.LogWarning("Booking not found: {BookingId}", request.BookingId);
                return false;
            }

            if (string.IsNullOrEmpty(booking.PaymentIntentId))
            {
                _logger.LogWarning("Booking has no payment intent: {BookingId}", request.BookingId);
                return false;
            }

            // Query Stripe for the current payment status
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
            var paymentIntentService = new PaymentIntentService();
            var paymentIntent = await paymentIntentService.GetAsync(booking.PaymentIntentId, cancellationToken: cancellationToken);

            _logger.LogInformation("Payment Intent Status for booking {BookingId}: {Status}", booking.Id, paymentIntent.Status);

            // Update booking based on Stripe status
            var statusChanged = false;
            switch (paymentIntent.Status)
            {
                case "succeeded":
                    if (booking.PaymentStatus != PaymentStatus.Completed)
                    {
                        booking.PaymentStatus = PaymentStatus.Completed;
                        booking.PaidAt = DateTime.UtcNow;
                        booking.Status = BookingStatus.Confirmed;
                        statusChanged = true;

                        _logger.LogInformation("Payment verified as succeeded for booking {BookingId}", booking.Id);

                        // Raise domain event for paid booking completion
                        // Calendar sync and notifications are handled via domain event handlers
                        booking.AddDomainEvent(new BookingCompletedEvent(
                            booking.Id,
                            booking.MeetlrEvent.Id,
                            booking.MeetlrEvent.UserId,
                            booking.TenantId,
                            wasPaidBooking: true));

                        // Save booking status changes and dispatch domain events
                        booking.UpdatedAt = DateTime.UtcNow;
                        await _unitOfWork.SaveChangesAsync(cancellationToken);

                        _logger.LogInformation("BookingCompletedEvent raised for paid booking {BookingId}", booking.Id);
                    }
                    break;

                case "canceled":
                    if (booking.PaymentStatus != PaymentStatus.Failed)
                    {
                        booking.PaymentStatus = PaymentStatus.Failed;
                        booking.Status = BookingStatus.Cancelled;
                        booking.CancelledAt = DateTime.UtcNow;
                        booking.CancellationReason = "Payment canceled";
                        statusChanged = true;

                        _logger.LogWarning("Payment verified as canceled for booking {BookingId}", booking.Id);
                    }
                    break;

                case "requires_payment_method":
                    // This is normal - payment form hasn't been submitted yet
                    // Don't mark as failed, just log it
                    _logger.LogInformation("Payment still requires payment method for booking {BookingId}", booking.Id);
                    break;

                case "processing":
                case "requires_action":
                case "requires_confirmation":
                    // Payment is still pending
                    _logger.LogInformation("Payment still pending for booking {BookingId}: {Status}", booking.Id, paymentIntent.Status);
                    break;
            }

            // Only save if status changed AND it's not already saved (for succeeded case)
            if (statusChanged && booking.PaymentStatus != PaymentStatus.Completed)
            {
                booking.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying payment status for booking {BookingId}", request.BookingId);
            return false;
        }
    }
}
