namespace Meetlr.Application.Features.MeetlrEvents.Commands.Create;

public record CreateMeetlrEventQuestionRequest
{
    public string QuestionText { get; init; } = string.Empty;
    public string Type { get; init; } = "Text";
    public bool IsRequired { get; init; }
    public int DisplayOrder { get; init; }
    public string? Options { get; init; }
}
