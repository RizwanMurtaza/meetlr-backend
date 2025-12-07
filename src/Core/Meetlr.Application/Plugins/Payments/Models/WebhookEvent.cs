namespace Meetlr.Application.Plugins.Payments.Models;

/// <summary>
/// Webhook event data
/// </summary>
public class WebhookEvent
{
    public string StripeEventType { get; set; } = string.Empty;
    public string PaymentIntentId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}
