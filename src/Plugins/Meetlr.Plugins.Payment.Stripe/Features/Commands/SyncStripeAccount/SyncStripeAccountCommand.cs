using MediatR;

namespace Meetlr.Plugins.Payment.Stripe.Features.Commands.SyncStripeAccount;

public class SyncStripeAccountCommand : IRequest<bool>
{
    public string StripeAccountId { get; set; } = string.Empty;
}
