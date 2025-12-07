namespace Meetlr.Domain.Events;

public class BookingCreatedEvent
{
    public Guid BookingId { get; set; }
    public Guid EventTypeId { get; set; }
    public Guid HostUserId { get; set; }
    public string InviteeEmail { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
