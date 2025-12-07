using Meetlr.Application.Features.Bookings.Commands.RescheduleBooking;

namespace Meetlr.Api.Endpoints.Bookings.Reschedule;

public class RescheduleBookingResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }

    // Updated booking details
    public DateTime NewStartTime { get; init; }
    public DateTime NewEndTime { get; init; }
    public int RescheduleCount { get; init; }

    public static RescheduleBookingResponse FromCommandResponse(RescheduleBookingCommandResponse response)
    {
        return new RescheduleBookingResponse
        {
            Success = response.Success,
            ErrorMessage = response.ErrorMessage,
            NewStartTime = response.NewStartTime,
            NewEndTime = response.NewEndTime,
            RescheduleCount = response.RescheduleCount
        };
    }
}
