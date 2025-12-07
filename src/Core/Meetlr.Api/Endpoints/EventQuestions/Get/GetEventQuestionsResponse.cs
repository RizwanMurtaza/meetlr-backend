namespace Meetlr.Api.Endpoints.EventQuestions.Get;

public class GetEventQuestionsResponse
{
    public Guid MeetlrEventId { get; set; }
    public List<QuestionDto> Questions { get; set; } = new();

    public static GetEventQuestionsResponse FromQueryResponse(
        Application.Features.MeetlrEvents.Queries.GetQuestions.GetEventQuestionsResponse response)
    {
        return new GetEventQuestionsResponse
        {
            MeetlrEventId = response.MeetlrEventId,
            Questions = response.Questions.Select(q => new QuestionDto
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                Type = q.Type,
                IsRequired = q.IsRequired,
                DisplayOrder = q.DisplayOrder,
                Options = q.Options
            }).ToList()
        };
    }
}

public class QuestionDto
{
    public Guid Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public string? Options { get; set; }
}
