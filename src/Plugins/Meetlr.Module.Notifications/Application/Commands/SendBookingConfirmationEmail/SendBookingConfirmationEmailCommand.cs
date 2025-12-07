using MediatR;

namespace Meetlr.Module.Notifications.Application.Commands.SendBookingConfirmationEmail;

/// <summary>
/// Command to send booking confirmation email using templates
/// </summary>
public record SendBookingConfirmationEmailCommand : IRequest<SendBookingConfirmationEmailCommandResponse>
{
    public Guid BookingId { get; init; }
}
