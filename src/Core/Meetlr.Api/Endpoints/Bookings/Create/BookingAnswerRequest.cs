namespace Meetlr.Api.Endpoints.Bookings.Create;

public class BookingAnswerRequest
{
    public Guid QuestionId { get; init; }
    public string Answer { get; init; } = string.Empty;
}
