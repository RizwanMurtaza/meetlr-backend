using MediatR;

namespace Meetlr.Application.Features.Bookings.Commands.CreateBooking;

/// <summary>
/// Command to create a new booking
/// </summary>
public record CreateBookingCommand : IRequest<CreateBookingCommandResponse>
{
    public Guid MeetlrEventId { get; init; }
    public string InviteeName { get; init; } = string.Empty;
    public string InviteeEmail { get; init; } = string.Empty;
    public string? InviteePhone { get; init; }
    public string? InviteeTimeZone { get; init; }
    public DateTime StartTime { get; init; }
    public string? Notes { get; init; }
    public List<CreateBookingAnswerRequest>? Answers { get; init; }

    // Series booking support
    public Guid? SeriesBookingId { get; init; }
    public bool SkipPayment { get; init; } = false;

    // Single-use booking link support
    public string? Token { get; init; }

    // Slot invitation support - when provided, pre-fills time slot and validates against invitation
    public string? SlotInvitationToken { get; init; }
}
