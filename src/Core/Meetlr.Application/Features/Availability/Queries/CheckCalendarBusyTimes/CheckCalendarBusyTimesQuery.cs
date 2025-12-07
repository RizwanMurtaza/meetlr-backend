using MediatR;

namespace Meetlr.Application.Features.Availability.Queries.CheckCalendarBusyTimes;

/// <summary>
/// Query to check if there are any calendar conflicts for a user during a time range
/// </summary>
public class CheckCalendarBusyTimesQuery : IRequest<CheckCalendarBusyTimesQueryResponse>
{
    public Guid UserId { get; set; }
    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }
}
