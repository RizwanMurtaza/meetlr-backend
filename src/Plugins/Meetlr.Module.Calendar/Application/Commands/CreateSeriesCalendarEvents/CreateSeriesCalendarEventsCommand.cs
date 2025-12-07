using MediatR;

namespace Meetlr.Module.Calendar.Application.Commands.CreateSeriesCalendarEvents;

public class CreateSeriesCalendarEventsCommand : IRequest<CreateSeriesCalendarEventsCommandResponse>
{
    public Guid MeetlrEventId { get; set; }
    public List<BookingEventInfo> Bookings { get; set; } = new();
}