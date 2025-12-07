namespace Meetlr.Application.Features.Payments.Commands.CreatePaymentIntent;

public class CreatePaymentIntentCommandResponse
{
    public bool Success { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? ClientSecret { get; set; }
    public string? PaymentUrl { get; set; }
    public string? ErrorMessage { get; set; }
}
