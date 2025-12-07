namespace Meetlr.Application.Features.Payments.Commands.CreateSeriesPayment;

public class CreateSeriesPaymentCommandResponse
{
    public bool Success { get; set; }
    public string? ClientSecret { get; set; }
    public string? SubscriptionId { get; set; }
    public decimal TotalAmount { get; set; }
    public string? ErrorMessage { get; set; }
}
