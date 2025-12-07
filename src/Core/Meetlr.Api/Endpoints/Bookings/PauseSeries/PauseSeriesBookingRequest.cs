namespace Meetlr.Api.Endpoints.Bookings.PauseSeries;

public record PauseSeriesBookingRequest
{
    public bool Resume { get; init; }
}
