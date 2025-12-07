namespace Meetlr.Api.Endpoints.MeetlrEvents.Create;

public class MeetlrEventQuestionRequest
{
    public string QuestionText { get; init; } = string.Empty;
    public string Type { get; init; } = "Text";
    public bool IsRequired { get; init; }
    public int DisplayOrder { get; init; }
    public string? Options { get; init; }
}
