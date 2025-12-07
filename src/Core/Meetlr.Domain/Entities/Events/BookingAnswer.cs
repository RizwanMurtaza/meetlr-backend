using Meetlr.Domain.Common;

namespace Meetlr.Domain.Entities.Events;

public class BookingAnswer : BaseEntity
{
    public Guid BookingId { get; set; }
    public Guid MeetlrEventQuestionId { get; set; }
    public string Answer { get; set; } = string.Empty;

    // Navigation properties
    public Booking Booking { get; set; } = null!;
    public MeetlrEventQuestion Question { get; set; } = null!;
}
