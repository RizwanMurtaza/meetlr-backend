namespace Meetlr.Application.Features.Bookings.Commands.RescheduleBooking;

public class RescheduleBookingCommandResponse
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    // Updated booking details
    public DateTime NewStartTime { get; set; }
    public DateTime NewEndTime { get; set; }
    public int RescheduleCount { get; set; }
}
