using MediatR;

namespace Meetlr.Application.Features.Bookings.Commands.RescheduleBooking;

public class RescheduleBookingCommand : IRequest<RescheduleBookingCommandResponse>
{
    public Guid BookingId { get; set; }
    public string ConfirmationToken { get; set; } = string.Empty;
    public string Identifier { get; set; } = string.Empty; // Email or phone for re-verification
    public DateTime NewStartTime { get; set; }
    public DateTime NewEndTime { get; set; }
}
