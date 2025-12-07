using MediatR;

namespace Meetlr.Module.Calendar.Application.Queries.GetCalendarBusyTimes;

public record GetCalendarBusyTimesQuery : IRequest<GetCalendarBusyTimesResponse>
{
    public Guid ScheduleId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}
