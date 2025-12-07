namespace Meetlr.Application.Features.Bookings.Commands.CreateRecurringBooking;

public record CreateBookingAnswerRequest
{
    public Guid QuestionId { get; init; }
    public string Answer { get; init; } = string.Empty;
}
