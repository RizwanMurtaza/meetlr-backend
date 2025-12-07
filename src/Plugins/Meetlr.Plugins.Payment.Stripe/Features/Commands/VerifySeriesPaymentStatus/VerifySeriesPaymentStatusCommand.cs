using MediatR;

namespace Meetlr.Plugins.Payment.Stripe.Features.Commands.VerifySeriesPaymentStatus;

public class VerifySeriesPaymentStatusCommand : IRequest<VerifySeriesPaymentStatusResponse>
{
    public Guid SeriesId { get; set; }
}