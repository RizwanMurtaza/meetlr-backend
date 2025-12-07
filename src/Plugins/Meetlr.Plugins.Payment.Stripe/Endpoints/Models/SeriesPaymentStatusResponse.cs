namespace Meetlr.Plugins.Payment.Stripe.Endpoints.Models;

public class SeriesPaymentStatusResponse
{
    public Guid SeriesId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public bool RequiresPayment { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Currency { get; set; }
    public DateTime? PaidAt { get; set; }
    public string EventTypeName { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public string InviteeName { get; set; } = string.Empty;
    public string InviteeEmail { get; set; } = string.Empty;
    public int TotalOccurrences { get; set; }
    public List<OccurrenceInfo> Occurrences { get; set; } = new();
}