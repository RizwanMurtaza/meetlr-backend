using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Plugins.Payment.Stripe.Features.Commands.ProcessStripeWebhook;

public class ProcessStripeWebhookCommandHandler : IRequestHandler<ProcessStripeWebhookCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly INotificationQueueService _notificationQueueService;
    private readonly ILogger<ProcessStripeWebhookCommandHandler> _logger;

    public ProcessStripeWebhookCommandHandler(
        IUnitOfWork unitOfWork,
        IMediator mediator,
        INotificationQueueService notificationQueueService,
        ILogger<ProcessStripeWebhookCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _notificationQueueService = notificationQueueService;
        _logger = logger;
    }

    public async Task<bool> Handle(ProcessStripeWebhookCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing Stripe webhook: {EventType} for PaymentIntent: {PaymentIntentId}", 
                request.EventType, request.PaymentIntentId);

            var booking = await _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .Include(b => b.MeetlrEvent)
                    .ThenInclude(et => et.User)
                .Include(b => b.Contact)
                .FirstOrDefaultAsync(b => b.PaymentIntentId == request.PaymentIntentId, cancellationToken);

            if (booking == null)
            {
                _logger.LogWarning("Booking not found for PaymentIntent: {PaymentIntentId}", request.PaymentIntentId);
                return false;
            }

            switch (request.EventType)
            {
                case "payment_intent.succeeded":
                    booking.PaymentStatus = PaymentStatus.Completed;
                    booking.PaidAt = DateTime.UtcNow;
                    booking.Status = BookingStatus.Confirmed;

                    _logger.LogInformation("Payment succeeded for booking {BookingId}", booking.Id);

                    // Save booking status changes first
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    // Create calendar event after payment confirmation
                    var inviteeName = booking.Contact?.Name ?? "Guest";
                    var inviteeEmail = booking.Contact?.Email ?? string.Empty;
                    var description = $"Booking confirmed via Calendly\n\nInvitee: {inviteeName}\nEmail: {inviteeEmail}\n";
                    if (!string.IsNullOrEmpty(booking.Notes))
                    {
                        description += $"\nNotes: {booking.Notes}";
                    }


                    // Queue booking notifications after successful payment
                    await _notificationQueueService.QueueBookingNotificationsAsync(
                        booking,
                        booking.MeetlrEvent,
                        NotificationTrigger.BookingCreated,
                        cancellationToken);

                    _logger.LogInformation("Booking notifications queued for booking {BookingId}", booking.Id);
                    break;

                case "payment_intent.payment_failed":
                    booking.PaymentStatus = PaymentStatus.Failed;
                    booking.Status = BookingStatus.Cancelled;
                    booking.CancelledAt = DateTime.UtcNow;
                    booking.CancellationReason = "Payment failed";

                    _logger.LogWarning("Payment failed for booking {BookingId}", booking.Id);
                    break;

                case "payment_intent.canceled":
                    booking.PaymentStatus = PaymentStatus.Failed;
                    booking.Status = BookingStatus.Cancelled;
                    booking.CancelledAt = DateTime.UtcNow;
                    booking.CancellationReason = "Payment cancelled";

                    _logger.LogInformation("Payment cancelled for booking {BookingId}", booking.Id);
                    break;

                case "charge.refunded":
                    booking.PaymentStatus = PaymentStatus.Refunded;
                    booking.RefundedAt = DateTime.UtcNow;

                    _logger.LogInformation("Payment refunded for booking {BookingId}", booking.Id);
                    break;

                default:
                    _logger.LogInformation("Unhandled webhook event type: {EventType}", request.EventType);
                    return true;
            }

            booking.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook");
            return false;
        }
    }
}
