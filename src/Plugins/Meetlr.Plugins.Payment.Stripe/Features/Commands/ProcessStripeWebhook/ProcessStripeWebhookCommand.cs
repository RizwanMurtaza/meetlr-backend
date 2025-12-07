using MediatR;

namespace Meetlr.Plugins.Payment.Stripe.Features.Commands.ProcessStripeWebhook;

public class ProcessStripeWebhookCommand : IRequest<bool>
{
    public string EventType { get; set; } = string.Empty;
    public string PaymentIntentId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}
