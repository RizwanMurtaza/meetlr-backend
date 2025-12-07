using Meetlr.Application.Features.Bookings.Commands.CreateBooking;

namespace Meetlr.Api.Endpoints.Bookings.Create;

public class CreateBookingRequest
{
    public Guid MeetlrEventId { get; init; }
    public string InviteeName { get; init; } = string.Empty;
    public string InviteeEmail { get; init; } = string.Empty;
    public string? InviteePhone { get; init; }
    public string? InviteeTimeZone { get; init; }
    public DateTime StartTime { get; init; }
    public string? Notes { get; init; }
    public List<BookingAnswerRequest>? Answers { get; init; }

    public static CreateBookingCommand ToCommand(CreateBookingRequest request)
    {
        return new CreateBookingCommand
        {
            MeetlrEventId = request.MeetlrEventId,
            InviteeName = request.InviteeName,
            InviteeEmail = request.InviteeEmail,
            InviteePhone = request.InviteePhone,
            InviteeTimeZone = request.InviteeTimeZone,
            StartTime = request.StartTime,
            Notes = request.Notes,
            Answers = request.Answers?.Select(a => new CreateBookingAnswerRequest
            {
                QuestionId = a.QuestionId,
                Answer = a.Answer
            }).ToList()
        };
    }
}
