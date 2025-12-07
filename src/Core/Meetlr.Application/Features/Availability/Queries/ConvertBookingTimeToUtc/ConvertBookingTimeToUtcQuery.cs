using MediatR;

namespace Meetlr.Application.Features.Availability.Queries.ConvertBookingTimeToUtc;

public class ConvertBookingTimeToUtcQuery : IRequest<ConvertBookingTimeToUtcQueryResponse>
{
    public DateTime RequestedTime { get; set; }
    public string TimeZone { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
}
