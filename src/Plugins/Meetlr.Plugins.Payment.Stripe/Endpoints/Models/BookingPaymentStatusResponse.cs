namespace Meetlr.Plugins.Payment.Stripe.Endpoints.Models;

public record BookingPaymentStatusResponse(
    Guid BookingId,
    string Status,
    decimal? Amount,
    string? Currency,
    string? PaymentIntentId,
    DateTime? PaidAt
);
