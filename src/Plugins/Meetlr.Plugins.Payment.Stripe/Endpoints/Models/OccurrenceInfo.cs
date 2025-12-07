namespace Meetlr.Plugins.Payment.Stripe.Endpoints.Models;

public class OccurrenceInfo
{
    public Guid BookingId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? MeetingLink { get; set; }
}