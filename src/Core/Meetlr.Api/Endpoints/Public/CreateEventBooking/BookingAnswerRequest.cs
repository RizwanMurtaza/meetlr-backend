namespace Meetlr.Api.Endpoints.Public.CreateEventBooking;

public class BookingAnswerRequest
{
    public Guid QuestionId { get; init; }
    public string Answer { get; init; } = string.Empty;
}
