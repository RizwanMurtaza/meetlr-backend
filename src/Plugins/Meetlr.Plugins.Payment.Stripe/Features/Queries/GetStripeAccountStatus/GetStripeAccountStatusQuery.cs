using MediatR;

namespace Meetlr.Plugins.Payment.Stripe.Features.Queries.GetStripeAccountStatus;

public class GetStripeAccountStatusQuery : IRequest<StripeAccountStatusResponse?>
{
}
