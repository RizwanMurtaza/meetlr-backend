using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Plugins.Payments;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Application.Features.Payments.Commands.CreateSeriesPayment;

/// <summary>
/// Command handler for creating series payments - 100% pure CQRS with no services
/// </summary>
public class CreateSeriesPaymentCommandHandler
    : IRequestHandler<CreateSeriesPaymentCommand, CreateSeriesPaymentCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly ILogger<CreateSeriesPaymentCommandHandler> _logger;

    public CreateSeriesPaymentCommandHandler(
        IUnitOfWork unitOfWork,
        IPaymentProviderFactory paymentProviderFactory,
        ILogger<CreateSeriesPaymentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _paymentProviderFactory = paymentProviderFactory;
        _logger = logger;
    }

    public async Task<CreateSeriesPaymentCommandResponse> Handle(
        CreateSeriesPaymentCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get event type with user and stripe account
            var spec = new Common.Specifications.MeetlrEventSpecifications.ByIdWithUserAndStripeAccount(request.MeetlrEventId);
            var eventType = await _unitOfWork.Repository<MeetlrEvent>()
                .FirstOrDefaultAsync(spec, cancellationToken);

            if (eventType == null)
            {
                _logger.LogWarning("eventType {MeetlrEventId} not found", request.MeetlrEventId);
                return new CreateSeriesPaymentCommandResponse
                {
                    Success = false,
                    ErrorMessage = "Event type not found"
                };
            }

            // Get series
            var series = await _unitOfWork.Repository<BookingSeries>()
                .GetByIdAsync(request.SeriesId, cancellationToken);

            if (series == null)
            {
                _logger.LogWarning("BookingSeries {SeriesId} not found", request.SeriesId);
                return new CreateSeriesPaymentCommandResponse
                {
                    Success = false,
                    ErrorMessage = "Booking series not found"
                };
            }

            // Get bookings
            var bookings = await _unitOfWork.Repository<Meetlr.Domain.Entities.Events.Booking>()
                .GetQueryable()
                .Where(b => request.BookingIds.Contains(b.Id))
                .ToListAsync(cancellationToken);

            // Inline payment logic (no service layer)
            if (eventType.PaymentProviderType == null)
            {
                throw PaymentErrors.PaymentProviderNotConfigured();
            }

            var plugin = _paymentProviderFactory.GetProvider(eventType.PaymentProviderType?.ToString())
                ?? throw PaymentErrors.PaymentProviderNotAvailable(eventType.PaymentProviderType?.ToString());

            // Check if user has payment account connected
            if (eventType.PaymentProviderType == PaymentProviderType.Stripe)
            {
                if (eventType.User.StripeAccount == null || !eventType.User.StripeAccount.IsConnected)
                {
                    throw PaymentErrors.PaymentAccountNotConnected("Stripe");
                }

                if (!eventType.User.StripeAccount.ChargesEnabled)
                {
                    throw PaymentErrors.PaymentAccountChargesDisabled("Stripe");
                }
            }

            string? clientSecret = null;
            string? subscriptionId = null;
            decimal totalAmount = 0;

            if (request.PaymentType == SeriesPaymentType.PerOccurrence)
            {
                // Create payment intent for all occurrences
                totalAmount = eventType.Fee!.Value * bookings.Count;
                var metadata = new Dictionary<string, string>
                {
                    { "series_id", series.Id.ToString() },
                    { "booking_type", "recurring_series" },
                    { "total_occurrences", bookings.Count.ToString() },
                    { "event_type", eventType.Name },
                    { "invitee_email", request.InviteeEmail }
                };

                string? connectedAccountId = null;
                if (eventType.PaymentProviderType == PaymentProviderType.Stripe && eventType.User.StripeAccount != null)
                {
                    connectedAccountId = eventType.User.StripeAccount.StripeAccountId;
                }

                var paymentIntent = await plugin.CreatePaymentIntentAsync(
                    series.Id,
                    totalAmount,
                    eventType.Currency ?? "usd",
                    connectedAccountId ?? string.Empty,
                    $"Recurring booking series - {bookings.Count} sessions",
                    metadata,
                    cancellationToken
                );

                // Set payment status pending for all bookings
                foreach (var booking in bookings)
                {
                    booking.PaymentStatus = PaymentStatus.Pending;
                    booking.Amount = eventType.Fee.Value;
                    booking.Currency = eventType.Currency;
                    booking.PaymentIntentId = paymentIntent.PaymentIntentId;
                }

                clientSecret = paymentIntent.ClientSecret;
            }
            else // Subscription
            {
                // TODO: Implement subscription payment
                // For now, mark as completed with placeholder
                series.SubscriptionId = $"sub_placeholder_{Guid.NewGuid()}";

                foreach (var booking in bookings)
                {
                    booking.PaymentStatus = PaymentStatus.Completed;
                    booking.Amount = eventType.Fee!.Value;
                    booking.Currency = eventType.Currency;
                    booking.PaidAt = DateTime.UtcNow;
                }

                subscriptionId = series.SubscriptionId;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreateSeriesPaymentCommandResponse
            {
                Success = true,
                ClientSecret = clientSecret,
                SubscriptionId = subscriptionId,
                TotalAmount = totalAmount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating series payment for Series {SeriesId}", request.SeriesId);

            return new CreateSeriesPaymentCommandResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
