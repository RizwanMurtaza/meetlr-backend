using Meetlr.Application.Features.Bookings.Commands.CreateRecurringBooking;
using Meetlr.Domain.Enums;

namespace Meetlr.Api.Endpoints.Public.CreateSeriesBooking;

public record CreateEventRecurringBookingRequest
{
    public Guid MeetlrEventId { get; set; }
    public string InviteeName { get; init; } = string.Empty;
    public string InviteeEmail { get; init; } = string.Empty;
    public string? InviteePhone { get; init; }
    public string? InviteeTimeZone { get; init; }
    public string? Notes { get; init; }

    // List of selected date/times for bookings (supports both recurring and bulk booking)
    public List<DateTime> SelectedDateTimes { get; init; } = new();

    public SeriesPaymentType PaymentType { get; init; }

    // Single-use booking link support
    public string? Token { get; init; }

    public static CreateRecurringBookingCommand ToCommand(CreateEventRecurringBookingRequest request)
    {
        return new CreateRecurringBookingCommand
        {
            MeetlrEventId = request.MeetlrEventId,
            InviteeName = request.InviteeName,
            InviteeEmail = request.InviteeEmail,
            InviteePhone = request.InviteePhone,
            InviteeTimeZone = request.InviteeTimeZone,
            Notes = request.Notes,
            SelectedDateTimes = request.SelectedDateTimes,
            PaymentType = request.PaymentType,
            Token = request.Token
        };
    }
}
