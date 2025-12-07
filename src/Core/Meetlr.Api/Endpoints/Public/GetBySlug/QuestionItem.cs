using Meetlr.Domain.Enums;

namespace Meetlr.Api.Endpoints.Public.GetBySlug;

public class QuestionItem
{
    public Guid Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public bool IsRequired { get; set; }
    public List<string> Options { get; set; } = new();
}
