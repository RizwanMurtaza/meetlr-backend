using MediatR;

namespace Meetlr.Module.Calendar.Application.Commands.DisconnectCalendar;

public class DisconnectCalendarCommand : IRequest<DisconnectCalendarResponse>
{
    public Guid CalendarIntegrationId { get; set; }
    public Guid UserId { get; set; }
}
