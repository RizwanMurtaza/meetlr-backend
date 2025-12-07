using Meetlr.Domain.Enums;

namespace Meetlr.Application.Features.Schedule.Queries.GetAvailabilitySchedules;

public class AvailabilityScheduleDto
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
    public List<CalendarIntegrationDto> CalendarIntegrations { get; set; } = new();

    public List<WeeklyHourDto> WeeklyHours { get; set; } = new();
    public List<DateOverrideDto> DateOverrides { get; set; } = new();
    public List<MeetlrEventDto> MeetlrEvents { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
