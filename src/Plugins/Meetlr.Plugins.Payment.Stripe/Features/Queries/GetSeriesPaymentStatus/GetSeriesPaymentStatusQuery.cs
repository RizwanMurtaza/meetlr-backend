using MediatR;

namespace Meetlr.Plugins.Payment.Stripe.Features.Queries.GetSeriesPaymentStatus;

public class GetSeriesPaymentStatusQuery : IRequest<SeriesPaymentStatusResponse?>
{
    public Guid SeriesId { get; set; }
}
