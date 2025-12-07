using Meetlr.Domain.Enums;

namespace Meetlr.Application.Features.MeetlrEvents.Queries.GetEventTypes;

public class MeetlrEventResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MeetingLocationType MeetingLocationType { get; set; }
    public string? LocationDetails { get; set; }
    public int DurationMinutes { get; set; }
    public int SlotIntervalMinutes { get; set; }
    public int BufferTimeBeforeMinutes { get; set; }
    public int BufferTimeAfterMinutes { get; set; }
    public int MinBookingNoticeMinutes { get; set; }
    public string? Color { get; set; }
    public MeetlrEventStatus Status { get; set; }
    public decimal? Fee { get; set; }
    public string? Currency { get; set; }
    public bool IsActive { get; set; }
    public Guid? AvailabilityScheduleId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Notification settings
    public bool NotifyViaEmail { get; set; }
    public bool NotifyViaSms { get; set; }
    public bool NotifyViaWhatsApp { get; set; }

    // Meeting type
    public MeetingType MeetingType { get; set; }
    public int? MaxAttendeesPerSlot { get; set; }

    // Payment
    public bool RequiresPayment { get; set; }
    public string? PaymentProviderType { get; set; }

    // Recurring
    public bool AllowsRecurring { get; set; }
    public int? MaxRecurringOccurrences { get; set; }

    // Booking statistics
    public int TotalBookings { get; set; }
    public int FutureBookings { get; set; }
}
