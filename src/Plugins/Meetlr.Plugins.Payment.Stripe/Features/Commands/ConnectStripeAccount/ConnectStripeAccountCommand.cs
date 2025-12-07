using MediatR;

namespace Meetlr.Plugins.Payment.Stripe.Features.Commands.ConnectStripeAccount;

public class ConnectStripeAccountCommand : IRequest<string>
{
    public string ReturnUrl { get; set; } = string.Empty;
}
