using Meetlr.Domain.Common;
using Meetlr.Domain.Entities.Contacts;
using Meetlr.Domain.Entities.Users;
using Meetlr.Domain.Enums;

namespace Meetlr.Domain.Entities.Events;

/// <summary>
/// Represents a series of bookings (recurring or bulk bookings)
/// Simplified to serve as a lightweight placeholder for grouping bookings
/// </summary>
public class BookingSeries : BaseAuditableEntity
{
    public Guid BaseMeetlrEventId { get; set; }
    public Guid HostUserId { get; set; }

    // Contact reference - all invitee info comes from Contact entity
    public Guid ContactId { get; set; }

    // Occurrence tracking
    public int OccurrenceCount { get; set; } // Actual number of bookings created
    public int TotalOccurrences { get; set; } // Expected number of bookings

    // Payment
    public SeriesPaymentType PaymentType { get; set; }
    public string? SubscriptionId { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.NotRequired;
    public PaymentProviderType? PaymentProviderType { get; set; }
    public decimal? TotalAmount { get; set; } // Total amount for all bookings in series
    public string? Currency { get; set; }
    public string? PaymentIntentId { get; set; } // Series-level payment intent ID
    public DateTime? PaidAt { get; set; }
    public DateTime? RefundedAt { get; set; }

    // Status
    public SeriesStatus Status { get; set; } = SeriesStatus.Active;

    // Navigation properties
    public Contact? Contact { get; set; }
    public MeetlrEvent BaseMeetlrEvent { get; set; } = null!;
    public User HostUser { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
