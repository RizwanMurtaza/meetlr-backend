namespace Meetlr.Application.Features.Bookings.Commands.CreateBooking;

public record CreateBookingAnswerRequest
{
    public Guid QuestionId { get; init; }
    public string Answer { get; init; } = string.Empty;
}
