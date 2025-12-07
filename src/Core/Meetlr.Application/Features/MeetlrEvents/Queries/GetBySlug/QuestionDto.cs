using Meetlr.Domain.Enums;

namespace Meetlr.Application.Features.MeetlrEvents.Queries.GetBySlug;

public class QuestionDto
{
    public Guid Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public bool IsRequired { get; set; }
    public List<string> Options { get; set; } = new();
}
