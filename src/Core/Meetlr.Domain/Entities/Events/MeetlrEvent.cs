using System.Text.Json.Serialization;
using Meetlr.Domain.Common;
using Meetlr.Domain.Entities.Scheduling;
using Meetlr.Domain.Entities.Users;
using Meetlr.Domain.Enums;

namespace Meetlr.Domain.Entities.Events;

public class MeetlrEvent : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public Guid AvailabilityScheduleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MeetingLocationType MeetingLocationType { get; set; } = MeetingLocationType.InPerson;
    public string? LocationDetails { get; set; } // Physical address for InPerson, meeting link for virtual, or phone number
    public int DurationMinutes { get; set; } = 30;
    public int SlotIntervalMinutes { get; set; } = 15; // Interval for generating available slots
    public int BufferTimeBeforeMinutes { get; set; } = 0;
    public int BufferTimeAfterMinutes { get; set; } = 0;
    public string? Color { get; set; } = "#0069ff"; // For calendar display
    public MeetlrEventStatus Status { get; set; } = MeetlrEventStatus.Active;
    public string Slug { get; set; } = string.Empty; // e.g., "30-minute-meeting"
    public bool IsActive { get; set; } = true;

    // Meeting type and capacity
    public MeetingType MeetingType { get; set; } = MeetingType.OneOnOne;
    public int? MaxAttendeesPerSlot { get; set; } // For Group meetings: max capacity per slot (e.g., 20 people)

    // Booking window
    public int MinBookingNoticeMinutes { get; set; } = 60; // Minimum notice before booking
    public int MaxBookingDaysInFuture { get; set; } = 60; // How far in advance can people book

    // Limits
    public int? MaxBookingsPerDay { get; set; }
    public int? MaxBookingsPerWeek { get; set; }

    // Payment settings (for slot fees)
    public bool RequiresPayment { get; set; }
    public decimal? Fee { get; set; }
    public string? Currency { get; set; } = "USD";
    public PaymentProviderType? PaymentProviderType { get; set; } // Selected payment provider for this event

    // Recurring bookings
    public bool AllowsRecurring { get; set; } = false;
    public int? MaxRecurringOccurrences { get; set; } = 10; // Maximum number of occurrences allowed

    // Confirmation & reminder settings
    public bool SendConfirmationEmail { get; set; } = true;
    public bool SendReminderEmail { get; set; } = true;
    public int ReminderHoursBefore { get; set; } = 24;
    public bool SendFollowUpEmail { get; set; }
    public int FollowUpHoursAfter { get; set; } = 2;

    // Notification preferences - Multi-channel notifications
    public bool NotifyViaEmail { get; set; } = true;
    public bool NotifyViaSms { get; set; } = false;
    public bool NotifyViaWhatsApp { get; set; } = false;

    // Navigation properties
    [JsonIgnore]
    public User User { get; set; } = null!;
    [JsonIgnore]
    public AvailabilitySchedule AvailabilitySchedule { get; set; } = null!;
    [JsonIgnore]
    public ICollection<MeetlrEventQuestion> Questions { get; set; } = new List<MeetlrEventQuestion>();
    [JsonIgnore]
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    [JsonIgnore]
    public EventTheme? Theme { get; set; }
}
