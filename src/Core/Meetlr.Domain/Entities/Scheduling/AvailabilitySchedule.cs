using Meetlr.Domain.Common;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Entities.Users;
using Meetlr.Domain.Enums;

namespace Meetlr.Domain.Entities.Scheduling;

public class AvailabilitySchedule : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = "Working hours (default)";
    public string TimeZone { get; set; } = "UTC";
    public bool IsDefault { get; set; }
    public ScheduleType ScheduleType { get; set; } = ScheduleType.Personal;
    public int MaxBookingsPerSlot { get; set; } = 1;

    // Advanced Settings (populated from UserSettings defaults when created)
    public int MaxBookingDaysInFuture { get; set; } = 60; // Booking Window - days into the future
    public int MinBookingNoticeMinutes { get; set; } = 60; // Minimum Notice - minutes (UI shows hours: 60 = 1 hour)
    public int SlotIntervalMinutes { get; set; } = 15; // Start Time Increments
    public bool AutoDetectInviteeTimezone { get; set; } = true; // Timezone Display: true = auto-detect, false = locked

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<WeeklyHours> WeeklyHours { get; set; } = new List<WeeklyHours>();
    public ICollection<DateSpecificHours> DateSpecificHours { get; set; } = new List<DateSpecificHours>();
    public ICollection<MeetlrEvent> MeetlrEvents { get; set; } = new List<MeetlrEvent>();
}
