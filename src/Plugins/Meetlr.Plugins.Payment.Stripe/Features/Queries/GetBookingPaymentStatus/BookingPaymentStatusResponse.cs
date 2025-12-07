namespace Meetlr.Plugins.Payment.Stripe.Features.Queries.GetBookingPaymentStatus;

public class BookingPaymentStatusResponse
{
    public Guid BookingId { get; set; }
    public string Status { get; set; } = string.Empty; // Confirmed, Pending, Failed, Cancelled
    public string PaymentStatus { get; set; } = string.Empty; // NotRequired, Pending, Completed, Failed, Refunded
    public bool RequiresPayment { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string EventTypeName { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public string InviteeName { get; set; } = string.Empty;
    public string InviteeEmail { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? MeetingLink { get; set; }
}
