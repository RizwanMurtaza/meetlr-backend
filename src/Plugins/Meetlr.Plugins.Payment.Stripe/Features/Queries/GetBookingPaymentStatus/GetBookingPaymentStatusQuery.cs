using MediatR;

namespace Meetlr.Plugins.Payment.Stripe.Features.Queries.GetBookingPaymentStatus;

public class GetBookingPaymentStatusQuery : IRequest<BookingPaymentStatusResponse?>
{
    public Guid BookingId { get; set; }
}
