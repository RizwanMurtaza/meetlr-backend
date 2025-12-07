using Meetlr.Application.Features.Bookings.Commands.CreateBooking;

namespace Meetlr.Api.Endpoints.Public.CreateEventBooking;

public class CreateEventBookingRequest
{
    public Guid MeetlrEventId { get; set; }
    public string InviteeName { get; init; } = string.Empty;
    public string InviteeEmail { get; init; } = string.Empty;
    public string? InviteePhone { get; init; }
    public string? InviteeTimeZone { get; init; }
    public DateTime StartTime { get; init; }
    public string? Notes { get; init; }
    public List<BookingAnswerRequest>? Answers { get; init; }
    public string? Token { get; init; } // Single-use booking link token
    public string? SlotInvitationToken { get; init; } // Slot invitation token for pre-filled bookings

    public static CreateBookingCommand ToCommand(CreateEventBookingRequest request)
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
            }).ToList(),
            Token = request.Token,
            SlotInvitationToken = request.SlotInvitationToken
        };
    }
}
