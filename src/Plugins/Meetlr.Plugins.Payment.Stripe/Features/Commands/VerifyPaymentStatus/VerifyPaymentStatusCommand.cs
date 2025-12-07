using MediatR;

namespace Meetlr.Plugins.Payment.Stripe.Features.Commands.VerifyPaymentStatus;

public class VerifyPaymentStatusCommand : IRequest<bool>
{
    public Guid BookingId { get; set; }
}
