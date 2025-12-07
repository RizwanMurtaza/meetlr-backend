using Meetlr.Domain.Enums;

namespace Meetlr.Api.Endpoints.Bookings.CancelSeries;

public record CancelSeriesBookingRequest
{
    public CancellationScope Scope { get; init; }
    public string Reason { get; init; } = string.Empty;
    public Guid? StartFromBookingId { get; init; }
}
