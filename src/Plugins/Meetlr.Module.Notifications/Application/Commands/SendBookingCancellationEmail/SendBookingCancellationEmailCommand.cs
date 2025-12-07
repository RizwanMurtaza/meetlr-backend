using MediatR;

namespace Meetlr.Module.Notifications.Application.Commands.SendBookingCancellationEmail;

/// <summary>
/// Command to send booking cancellation email using templates
/// </summary>
public record SendBookingCancellationEmailCommand : IRequest<SendBookingCancellationEmailCommandResponse>
{
    public Guid BookingId { get; init; }
}
