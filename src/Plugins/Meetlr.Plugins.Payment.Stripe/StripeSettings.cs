namespace Meetlr.Plugins.Payment.Stripe;

public class StripeSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string PublishableKey { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public int ApplicationFeePercent { get; set; } = 10;
}
