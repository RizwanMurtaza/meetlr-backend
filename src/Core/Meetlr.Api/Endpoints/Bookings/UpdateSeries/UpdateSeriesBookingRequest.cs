using Meetlr.Domain.Enums;

namespace Meetlr.Api.Endpoints.Bookings.UpdateSeries;

public record UpdateSeriesBookingRequest
{
    public CancellationScope Scope { get; init; }
    public DateTime? NewStartTime { get; init; }
    public int? NewDuration { get; init; }
    public Guid? StartFromBookingId { get; init; }
}
