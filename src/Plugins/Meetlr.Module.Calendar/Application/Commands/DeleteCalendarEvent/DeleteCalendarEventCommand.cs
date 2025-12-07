using MediatR;

namespace Meetlr.Module.Calendar.Application.Commands.DeleteCalendarEvent;

public record DeleteCalendarEventCommand : IRequest<DeleteCalendarEventResponse>
{
    public Guid ScheduleId { get; init; }
    public string CalendarEventId { get; init; } = string.Empty;
}
