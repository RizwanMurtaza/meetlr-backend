using MediatR;

namespace Meetlr.Plugins.Payment.Stripe.Features.Commands.CompleteStripeConnect;

public class CompleteStripeConnectCommand : IRequest<bool>
{
    public string AuthorizationCode { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
}
