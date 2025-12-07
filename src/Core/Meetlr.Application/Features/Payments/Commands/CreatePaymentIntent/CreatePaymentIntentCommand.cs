using MediatR;

namespace Meetlr.Application.Features.Payments.Commands.CreatePaymentIntent;

public class CreatePaymentIntentCommand : IRequest<CreatePaymentIntentCommandResponse>
{
    public Guid BookingId { get; set; }
    public Guid MeetlrEventId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentProviderType { get; set; } = string.Empty;
    public Dictionary<string, string>? Metadata { get; set; }
}
