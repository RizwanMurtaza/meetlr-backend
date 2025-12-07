using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Plugins.Payments;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Entities.Notifications;
using Meetlr.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Meetlr.Application.Features.Payments.Commands.ProcessRefund;

/// <summary>
/// Handler responsible ONLY for processing refunds.
/// Does NOT manage pending/history tables - that's the background service's responsibility.
/// </summary>
public class ProcessRefundCommandHandler : IRequestHandler<ProcessRefundCommand, ProcessRefundCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly ILogger<ProcessRefundCommandHandler> _logger;

    public ProcessRefundCommandHandler(
        IUnitOfWork unitOfWork,
        IPaymentProviderFactory paymentProviderFactory,
        ILogger<ProcessRefundCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _paymentProviderFactory = paymentProviderFactory;
        _logger = logger;
    }

    public async Task<ProcessRefundCommandResponse> Handle(
        ProcessRefundCommand request,
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
                    "Notification {NotificationId} not found for refund processing",
                    request.NotificationPendingId);

                return new ProcessRefundCommandResponse
                {
                    Success = false,
                    ErrorMessage = "Notification not found"
                };
            }

            // 2. Get Booking by BookingId (has all payment details)
            if (!notification.BookingId.HasValue)
            {
                _logger.LogWarning(
                    "Notification {NotificationId} has no BookingId for refund processing",
                    request.NotificationPendingId);

                return new ProcessRefundCommandResponse
                {
                    Success = false,
                    ErrorMessage = "Notification has no associated booking"
                };
            }

            var booking = await _unitOfWork.Repository<Booking>()
                .GetByIdAsync(notification.BookingId.Value, cancellationToken);

            if (booking == null)
            {
                _logger.LogWarning(
                    "Booking {BookingId} not found for refund processing",
                    notification.BookingId);

                return new ProcessRefundCommandResponse
                {
                    Success = false,
                    ErrorMessage = "Booking not found"
                };
            }

            // 3. Validate booking has payment details
            if (string.IsNullOrEmpty(booking.PaymentIntentId))
            {
                _logger.LogWarning(
                    "Booking {BookingId} has no PaymentIntentId for refund",
                    booking.Id);

                return new ProcessRefundCommandResponse
                {
                    Success = false,
                    ErrorMessage = "No PaymentIntentId found"
                };
            }

            // 4. Get payment provider
            var providerType = booking.PaymentProviderType?.ToString() ?? "Stripe";
            var provider = _paymentProviderFactory.GetProvider(providerType);

            if (provider == null)
            {
                _logger.LogError(
                    "Payment provider {ProviderType} not found for booking {BookingId}",
                    providerType, booking.Id);

                return new ProcessRefundCommandResponse
                {
                    Success = false,
                    ErrorMessage = $"Payment provider {providerType} not found"
                };
            }

            _logger.LogInformation(
                "Processing refund for booking {BookingId}, PaymentIntentId: {PaymentIntentId}, Amount: {Amount}",
                booking.Id, booking.PaymentIntentId, booking.Amount);

            // 5. Call Stripe refund
            var success = await provider.RefundPaymentAsync(
                booking.PaymentIntentId,
                booking.Amount,
                cancellationToken);

            if (success)
            {
                _logger.LogInformation(
                    "Refund successful for booking {BookingId}, Amount: {Amount} {Currency}",
                    booking.Id, booking.Amount, booking.Currency);

                // 6. Update booking status
                booking.PaymentStatus = PaymentStatus.Refunded;
                booking.RefundedAt = DateTime.UtcNow;
                booking.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<Booking>().Update(booking);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new ProcessRefundCommandResponse
                {
                    Success = true
                };
            }
            else
            {
                _logger.LogWarning(
                    "Refund failed for booking {BookingId}, PaymentIntentId: {PaymentIntentId}",
                    booking.Id, booking.PaymentIntentId);

                return new ProcessRefundCommandResponse
                {
                    Success = false,
                    ErrorMessage = "Refund API call failed"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Exception while processing refund for notification {NotificationId}",
                request.NotificationPendingId);

            return new ProcessRefundCommandResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
