using MediatR;
using Meetlr.Domain.Enums;

namespace Meetlr.Application.Features.Bookings.Commands.CreateRecurringBooking;

public record CreateRecurringBookingCommand : IRequest<CreateRecurringBookingCommandResponse>
{
    // Base booking fields
    public Guid MeetlrEventId { get; init; }
    public string InviteeName { get; init; } = string.Empty;
    public string InviteeEmail { get; init; } = string.Empty;
    public string? InviteePhone { get; init; }
    public string? InviteeTimeZone { get; init; }
    public string? Notes { get; init; }

    // List of selected date/times (supports both recurring and bulk booking)
    public List<DateTime> SelectedDateTimes { get; init; } = new();

    public SeriesPaymentType PaymentType { get; init; }

    // Single-use booking link support
    public string? Token { get; init; }
}
