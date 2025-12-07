using Meetlr.Domain.Common;
using Meetlr.Domain.Enums;

namespace Meetlr.Domain.Entities.Events;

public class MeetlrEventQuestion : BaseEntity
{
    public Guid MeetlrEventId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public QuestionType Type { get; set; } = QuestionType.Text;
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public string? Options { get; set; } // JSON array for choice questions

    // Navigation properties
    public MeetlrEvent MeetlrEvent { get; set; } = null!;
    public ICollection<BookingAnswer> Answers { get; set; } = new List<BookingAnswer>();
}
