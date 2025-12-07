using MediatR;

namespace Meetlr.Module.Calendar.Application.Commands.UpdateCalendarSettings;

public class UpdateCalendarSettingsCommand : IRequest<UpdateCalendarSettingsCommandResponse>
{
    public Guid ScheduleId { get; set; }
    public Guid CalendarIntegrationId { get; set; }
    public bool IsPrimaryCalendar { get; set; }
    public bool CheckForConflicts { get; set; }
    public bool AddEventsToCalendar { get; set; }
    public bool IncludeBuffers { get; set; }
    public bool AutoSync { get; set; }
}
