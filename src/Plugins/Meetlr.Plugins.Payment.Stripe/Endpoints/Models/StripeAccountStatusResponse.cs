namespace Meetlr.Plugins.Payment.Stripe.Endpoints.Models;

public record StripeAccountStatusResponse(
    bool IsConnected,
    string? AccountId,
    string? Email,
    bool? ChargesEnabled,
    bool? PayoutsEnabled
);
