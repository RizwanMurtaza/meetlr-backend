using System.Text.Json;
using Meetlr.Domain.Common;
using Meetlr.Domain.Entities.Contacts;
using Meetlr.Domain.Entities.Users;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Events;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;

namespace Meetlr.Domain.Entities.Events;

public class Booking : BaseAuditableEntity
{
    public Guid MeetlrEventId { get; set; }
    public Guid HostUserId { get; set; }
    public Guid? SeriesBookingId { get; set; } // Links to recurring series

    // Contact reference - all invitee info comes from Contact entity
    public Guid ContactId { get; set; }

    // Booking details
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    // Status with auto domain event raising
    private BookingStatus _status = BookingStatus.Pending;
    public BookingStatus Status
    {
        get => _status;
        set
        {
            var oldStatus = _status;
            _status = value;

            // Auto-raise BookingCompletedEvent when status becomes Confirmed
            if (oldStatus != BookingStatus.Confirmed && value == BookingStatus.Confirmed)
            {
                AddDomainEvent(new BookingCompletedEvent(
                    Id, MeetlrEventId, HostUserId, TenantId,
                    wasPaidBooking: oldStatus == BookingStatus.Pending));
            }

            // Auto-raise BookingCancelledEvent when status becomes Cancelled
            if (oldStatus != BookingStatus.Cancelled && value == BookingStatus.Cancelled)
            {
                AddDomainEvent(new BookingCancelledEvent(
                    Id, MeetlrEventId, HostUserId, TenantId,
                    CancellationReason));
            }
        }
    }

    public string? Location { get; set; }
    public string? MeetingLink { get; set; } // Zoom, Google Meet, etc.
    public string? MeetingId { get; set; } // External meeting ID for video conferencing (used for deletion)
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Payment information
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.NotRequired;
    public PaymentProviderType? PaymentProviderType { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
    public string? PaymentIntentId { get; set; } // Stripe/PayPal payment ID
    public DateTime? PaidAt { get; set; }
    public DateTime? RefundedAt { get; set; }

    // Series Payment Tracking (for bookings that are part of a recurring series)
    public string? SeriesPaymentIntentId { get; set; } // Links to the series payment intent
    public decimal? AllocatedAmount { get; set; } // Portion of series payment allocated to this booking
    public int? OccurrenceIndex { get; set; } // Index in the series (1-based: 1st, 2nd, 3rd occurrence)

    // Tracking
    public string ConfirmationToken { get; set; } = Guid.NewGuid().ToString();
    public string CancellationToken { get; set; } = Guid.NewGuid().ToString();
    public string? CalendarEventId { get; set; } // External calendar event ID (JSON for multiple providers)
    public bool ReminderSent { get; set; }
    public DateTime? ReminderSentAt { get; set; }
    public bool FollowUpSent { get; set; }
    public DateTime? FollowUpSentAt { get; set; }

    // Rescheduling tracking
    public Guid? RescheduledFromBookingId { get; set; } // Original booking if this is a rescheduled booking
    public Guid? RescheduledToBookingId { get; set; } // New booking if this one was rescheduled
    public int RescheduleCount { get; set; } = 0; // Number of times this booking has been rescheduled
    public string? RescheduleHistoryJson { get; set; } // JSON array of reschedule history entries
    public string? CancelledBy { get; set; } // "Host" or "Invitee"

    // No-show tracking
    public bool IsNoShow { get; set; } = false;
    public DateTime? NoShowAt { get; set; }
    public Guid? NoShowMarkedBy { get; set; } // UserId who marked as no-show
    public string? NoShowNotes { get; set; }

    // Check-in tracking
    public bool IsCheckedIn { get; set; } = false;
    public DateTime? CheckedInAt { get; set; }
    public string? CheckInMethod { get; set; } // "Link", "QRCode", "Manual", etc.

    // Recording tracking
    public bool RecordingEnabled { get; set; } = false;
    public string? RecordingUrl { get; set; }
    public string? TranscriptUrl { get; set; }
    public string? RecordingStatus { get; set; } // "Processing", "Ready", "Failed", etc.

    // Source tracking
    public string? BookingSource { get; set; } // "Website", "API", "Mobile", "Integration", etc.
    public string? ReferrerUrl { get; set; }
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmCampaign { get; set; }

    // Additional payment details
    public decimal? DiscountAmount { get; set; }
    public string? DiscountCode { get; set; }
    public decimal? TaxAmount { get; set; }

    // Internal notes and custom data
    public string? InternalNotes { get; set; } // Private notes visible only to host
    public string? CustomFieldsJson { get; set; } // JSON for custom fields/metadata

    // Navigation properties
    public Contact? Contact { get; set; }
    public MeetlrEvent MeetlrEvent { get; set; } = null!;
    public User HostUser { get; set; } = null!;
    public BookingSeries? BookingSeries { get; set; }
    public ICollection<BookingAnswer> Answers { get; set; } = new List<BookingAnswer>();

    /// <summary>
    /// Reschedules the booking to a new time slot.
    /// Validates: 1 reschedule limit, 72-hour rule, and confirmed status.
    /// Auto-raises BookingRescheduledEvent on success.
    /// </summary>
    public void Reschedule(DateTime newStartTime, DateTime newEndTime)
    {
        // Validate: Only 1 reschedule allowed
        if (RescheduleCount >= 1)
            throw BookingErrors.RescheduleLimitExceeded(RescheduleCount);

        // Validate: Current booking must be more than 72 hours away
        if (StartTime <= DateTime.UtcNow.AddHours(72))
            throw BookingErrors.RescheduleNotAllowedWithin72Hours(StartTime);

        // Validate: Only confirmed bookings can be rescheduled
        if (Status != BookingStatus.Confirmed)
            throw BookingErrors.BookingNotConfirmed(Status);

        var oldStartTime = StartTime;
        var oldEndTime = EndTime;

        // Record reschedule history
        AddRescheduleHistory(oldStartTime, oldEndTime, newStartTime, newEndTime);

        // Update booking times
        StartTime = newStartTime;
        EndTime = newEndTime;
        RescheduleCount++;
        UpdatedAt = DateTime.UtcNow;

        // Raise domain event for calendar update and notification
        AddDomainEvent(new BookingRescheduledEvent(
            Id, MeetlrEventId, HostUserId, TenantId,
            oldStartTime, oldEndTime, newStartTime, newEndTime));
    }

    /// <summary>
    /// Adds a reschedule history entry to the JSON array
    /// </summary>
    private void AddRescheduleHistory(DateTime oldStartTime, DateTime oldEndTime, DateTime newStartTime, DateTime newEndTime)
    {
        var historyEntry = new RescheduleHistoryEntry
        {
            RescheduledAt = DateTime.UtcNow,
            OldStartTime = oldStartTime,
            OldEndTime = oldEndTime,
            NewStartTime = newStartTime,
            NewEndTime = newEndTime
        };

        var history = string.IsNullOrEmpty(RescheduleHistoryJson)
            ? new List<RescheduleHistoryEntry>()
            : JsonSerializer.Deserialize<List<RescheduleHistoryEntry>>(RescheduleHistoryJson) ?? new List<RescheduleHistoryEntry>();

        history.Add(historyEntry);
        RescheduleHistoryJson = JsonSerializer.Serialize(history);
    }
}

/// <summary>
/// Represents a single reschedule history entry
/// </summary>
public class RescheduleHistoryEntry
{
    public DateTime RescheduledAt { get; set; }
    public DateTime OldStartTime { get; set; }
    public DateTime OldEndTime { get; set; }
    public DateTime NewStartTime { get; set; }
    public DateTime NewEndTime { get; set; }
}
