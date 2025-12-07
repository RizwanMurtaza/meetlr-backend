namespace Meetlr.Application.Features.Availability.Queries.CheckPastBooking;

public class CheckPastBookingQueryResponse
{
    public bool IsInPast { get; set; }
    public string? ErrorMessage { get; set; }
}
