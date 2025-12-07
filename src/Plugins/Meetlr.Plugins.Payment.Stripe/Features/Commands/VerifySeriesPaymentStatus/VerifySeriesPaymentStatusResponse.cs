namespace Meetlr.Plugins.Payment.Stripe.Features.Commands.VerifySeriesPaymentStatus;

public class VerifySeriesPaymentStatusResponse
{
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}