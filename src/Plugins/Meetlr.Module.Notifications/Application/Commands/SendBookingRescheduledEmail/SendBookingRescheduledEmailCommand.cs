using MediatR;

namespace Meetlr.Module.Notifications.Application.Commands.SendBookingRescheduledEmail;

/// <summary>
/// Command to send booking rescheduled email using templates.
/// Shows old vs new time and includes updated calendar invite.
/// </summary>
public record SendBookingRescheduledEmailCommand : IRequest<SendBookingRescheduledEmailCommandResponse>
{
    public Guid BookingId { get; init; }

    /// <summary>
    /// The original start time before reschedule
    /// </summary>
    public DateTime OldStartTime { get; init; }

    /// <summary>
    /// The original end time before reschedule
    /// </summary>
    public DateTime OldEndTime { get; init; }
}
