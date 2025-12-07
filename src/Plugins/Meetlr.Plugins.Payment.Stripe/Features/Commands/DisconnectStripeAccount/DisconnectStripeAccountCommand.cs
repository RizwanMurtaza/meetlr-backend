using MediatR;

namespace Meetlr.Plugins.Payment.Stripe.Features.Commands.DisconnectStripeAccount;

public class DisconnectStripeAccountCommand : IRequest<bool>
{
}
