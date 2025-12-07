using MediatR;

namespace Meetlr.Module.Notifications.Application.Commands.SendBookingReminderEmail;

/// <summary>
/// Command to send booking reminder email using templates
/// </summary>
public record SendBookingReminderEmailCommand : IRequest<SendBookingReminderEmailCommandResponse>
{
    public Guid BookingId { get; init; }
}
