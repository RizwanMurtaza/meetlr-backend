using Meetlr.Domain.Common;

namespace Meetlr.Domain.Events;

/// <summary>
/// Domain event raised when a booking status becomes Cancelled.
/// This triggers:
/// 1. Video meeting deletion (if exists)
/// 2. Calendar event deletion (if exists)
/// 3. Refund processing (if payment was completed)
/// 4. Cancellation notification
/// </summary>
public class BookingCancelledEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>
    /// The booking that was cancelled
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
    /// Optional cancellation reason
    /// </summary>
    public string? CancellationReason { get; }

    public BookingCancelledEvent(
        Guid bookingId,
        Guid meetlrEventId,
        Guid hostUserId,
        Guid tenantId,
        string? cancellationReason = null)
    {
        BookingId = bookingId;
        MeetlrEventId = meetlrEventId;
        HostUserId = hostUserId;
        TenantId = tenantId;
        CancellationReason = cancellationReason;
    }
}
