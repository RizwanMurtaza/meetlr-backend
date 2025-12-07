namespace Meetlr.Plugins.Payment.Stripe.Features.Queries.GetStripeAccountStatus;

public class StripeAccountStatusResponse
{
    public bool IsConnected { get; set; }
    public bool ChargesEnabled { get; set; }
    public bool PayoutsEnabled { get; set; }
    public bool DetailsSubmitted { get; set; }
    public string? VerificationStatus { get; set; }
    public string? DisabledReason { get; set; }
    public string? Country { get; set; }
    public string? Currency { get; set; }
    public DateTime? ConnectedAt { get; set; }
}
