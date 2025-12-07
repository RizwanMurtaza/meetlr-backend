using Meetlr.Domain.Enums;

namespace Meetlr.Api.Endpoints.Schedule.Get;

public class AvailabilityScheduleItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public ScheduleType ScheduleType { get; set; }
    public int MaxBookingsPerSlot { get; set; }

    // Advanced Settings
    public int MaxBookingDaysInFuture { get; set; }
    public int MinBookingNoticeMinutes { get; set; }
    public int SlotIntervalMinutes { get; set; }
    public bool AutoDetectInviteeTimezone { get; set; }

    // Calendar Integrations (multiple calendars per schedule)
    public List<CalendarIntegrationItem> CalendarIntegrations { get; set; } = new();

    public List<WeeklyHourItem> WeeklyHours { get; set; } = new();
    public List<DateOverrideItem> DateOverrides { get; set; } = new();
    public List<AvailabilityEventTypeItem> MeetlrEvents { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
