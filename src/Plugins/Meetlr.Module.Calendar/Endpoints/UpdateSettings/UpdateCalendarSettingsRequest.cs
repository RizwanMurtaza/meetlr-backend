namespace Meetlr.Module.Calendar.Endpoints.UpdateSettings;

public class UpdateCalendarSettingsRequest
{
    public Guid ScheduleId { get; set; }
    public bool IsPrimaryCalendar { get; set; }
    public bool CheckForConflicts { get; set; }
    public bool AddEventsToCalendar { get; set; }
    public bool IncludeBuffers { get; set; }
    public bool AutoSync { get; set; }
}
