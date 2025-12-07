using MediatR;

namespace Meetlr.Application.Features.Payments.Queries.GetStripeAccountStatus;

public class GetStripeAccountStatusQuery : IRequest<StripeAccountStatusResponse?>
{
}
