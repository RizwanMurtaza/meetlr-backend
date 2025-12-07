using MediatR;

namespace Meetlr.Module.Calendar.Application.Queries.GetConnectedCalendars;

public record GetConnectedCalendarsQuery(Guid ScheduleId) : IRequest<GetConnectedCalendarsResponse>;
