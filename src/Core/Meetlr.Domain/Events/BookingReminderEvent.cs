namespace Meetlr.Domain.Events;

public class BookingReminderEvent
{
    public Guid BookingId { get; set; }
    public string InviteeEmail { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
}
