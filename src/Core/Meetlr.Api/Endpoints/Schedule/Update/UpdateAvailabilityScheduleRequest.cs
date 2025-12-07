using Meetlr.Domain.Enums;

namespace Meetlr.Api.Endpoints.Schedule.Update;

public class UpdateAvailabilityScheduleRequest
{
    public string Name { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public ScheduleType ScheduleType { get; set; } = ScheduleType.Personal;
    public int MaxBookingsPerSlot { get; set; } = 1;
    public List<WeeklyHourRequest> WeeklyHours { get; set; } = new();

    // Advanced Settings
    public int MaxBookingDaysInFuture { get; set; } = 60;
    public int MinBookingNoticeMinutes { get; set; } = 60;
    public int SlotIntervalMinutes { get; set; } = 15;
    public bool AutoDetectInviteeTimezone { get; set; } = true;
}
