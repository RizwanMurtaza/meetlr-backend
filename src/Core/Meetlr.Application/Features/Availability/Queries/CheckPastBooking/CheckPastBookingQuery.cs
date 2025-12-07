using MediatR;

namespace Meetlr.Application.Features.Availability.Queries.CheckPastBooking;

/// <summary>
/// Query to check if a booking time is in the past
/// </summary>
public class CheckPastBookingQuery : IRequest<CheckPastBookingQueryResponse>
{
    public DateTime StartTimeUtc { get; set; }
}
