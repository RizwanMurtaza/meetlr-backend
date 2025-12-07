using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Enums;
using Meetlr.Plugins.Payment.Stripe.Features.Commands.VerifyPaymentStatus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Scheduler.Services.Payments;

/// <summary>
/// Service for verifying pending payments by querying Stripe directly.
/// This is a fallback for when webhooks fail to update payment status.
/// Only checks bookings created in the last 30 minutes with a PaymentIntentId.
/// </summary>
public class PaymentVerificationService : IPaymentVerificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentVerificationService> _logger;

    public PaymentVerificationService(
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger<PaymentVerificationService> logger)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task VerifyPendingPaymentsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var thirtyMinutesAgo = DateTime.UtcNow.AddMinutes(-30);

            // Find bookings that:
            // - Have a PaymentIntentId (payment was initiated)
            // - PaymentStatus is Pending or Initiated (not yet confirmed)
            // - BookingStatus is Pending (not yet confirmed)
            // - Created in the last 30 minutes (recent enough to be worth checking)
            var pendingPayments = await _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .Where(b =>
                    !string.IsNullOrEmpty(b.PaymentIntentId) &&
                    (b.PaymentStatus == PaymentStatus.Pending || b.PaymentStatus == PaymentStatus.Initiated) &&
                    b.Status == BookingStatus.Pending &&
                    b.CreatedAt >= thirtyMinutesAgo)
                .ToListAsync(cancellationToken);

            if (!pendingPayments.Any())
            {
                _logger.LogDebug("No pending payments to verify");
                return;
            }

            _logger.LogInformation("Verifying {Count} pending payments with Stripe", pendingPayments.Count);

            foreach (var booking in pendingPayments)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    await _mediator.Send(new VerifyPaymentStatusCommand { BookingId = booking.Id }, cancellationToken);
                    _logger.LogInformation("Verified payment status for booking {BookingId}", booking.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to verify payment status for booking {BookingId}", booking.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during payment verification");
        }
    }
}
