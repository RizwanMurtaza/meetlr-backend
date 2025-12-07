using Meetlr.Domain.Common;

namespace Meetlr.Domain.Events;

/// <summary>
/// Domain event raised when a booking status becomes Completed (confirmed).
/// This triggers:
/// 1. Calendar sync (if auto-sync is enabled)
/// 2. Confirmation notification (with meeting link and notes)
/// 3. Reminder notification scheduling
/// 4. Follow-up notification scheduling
/// </summary>
public class BookingCompletedEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>
    /// The booking that was completed/confirmed
    /// </summary>
    public Guid BookingId { get; }

    /// <summary>
    /// The MeetlrEvent this booking belongs to
    /// </summary>
    public Guid MeetlrEventId { get; }

    /// <summary>
    /// The host user who owns the event
    /// </summary>
    public Guid HostUserId { get; }

    /// <summary>
    /// The tenant ID for multi-tenancy
    /// </summary>
    public Guid TenantId { get; }

    /// <summary>
    /// Whether this booking was from a paid event (payment just completed)
    /// </summary>
    public bool WasPaidBooking { get; }

    public BookingCompletedEvent(
        Guid bookingId,
        Guid meetlrEventId,
        Guid hostUserId,
        Guid tenantId,
        bool wasPaidBooking = false)
    {
        BookingId = bookingId;
        MeetlrEventId = meetlrEventId;
        HostUserId = hostUserId;
        TenantId = tenantId;
        WasPaidBooking = wasPaidBooking;
    }
}
