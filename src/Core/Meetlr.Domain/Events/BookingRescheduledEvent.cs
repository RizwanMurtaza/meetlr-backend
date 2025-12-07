using Meetlr.Domain.Common;

namespace Meetlr.Domain.Events;

/// <summary>
/// Domain event raised when a booking is rescheduled to a new time.
/// This triggers:
/// 1. Calendar event update (delete old + create new)
/// 2. Reschedule confirmation notification
/// </summary>
public class BookingRescheduledEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>
    /// The booking that was rescheduled
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
    /// The original start time before rescheduling
    /// </summary>
    public DateTime OldStartTime { get; }

    /// <summary>
    /// The original end time before rescheduling
    /// </summary>
    public DateTime OldEndTime { get; }

    /// <summary>
    /// The new start time after rescheduling
    /// </summary>
    public DateTime NewStartTime { get; }

    /// <summary>
    /// The new end time after rescheduling
    /// </summary>
    public DateTime NewEndTime { get; }

    public BookingRescheduledEvent(
        Guid bookingId,
        Guid meetlrEventId,
        Guid hostUserId,
        Guid tenantId,
        DateTime oldStartTime,
        DateTime oldEndTime,
        DateTime newStartTime,
        DateTime newEndTime)
    {
        BookingId = bookingId;
        MeetlrEventId = meetlrEventId;
        HostUserId = hostUserId;
        TenantId = tenantId;
        OldStartTime = oldStartTime;
        OldEndTime = oldEndTime;
        NewStartTime = newStartTime;
        NewEndTime = newEndTime;
    }
}
