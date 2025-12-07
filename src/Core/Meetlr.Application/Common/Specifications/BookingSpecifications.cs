using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Enums;

namespace Meetlr.Application.Common.Specifications;

/// <summary>
/// Specifications for Booking queries
/// </summary>
public static class BookingSpecifications
{
    public class ById : BaseSpecification<Booking>
    {
        public ById(Guid bookingId)
            : base(b => b.Id == bookingId)
        {
        }
    }

    public class ByIdWithDetails : BaseSpecification<Booking>
    {
        public ByIdWithDetails(Guid bookingId)
            : base(b => b.Id == bookingId)
        {
            AddInclude(x => x.MeetlrEvent);
            AddInclude(x => x.HostUser);
        }
    }

    public class ByIdWithEventTypeAndUser : BaseSpecification<Booking>
    {
        public ByIdWithEventTypeAndUser(Guid bookingId)
            : base(b => b.Id == bookingId)
        {
            AddInclude(x => x.MeetlrEvent);
            AddInclude(x => x.HostUser);
        }
    }

    public class ConflictingBookingForSlot : BaseSpecification<Booking>
    {
        public ConflictingBookingForSlot(Guid MeetlrEventId, DateTime startTime, DateTime endTime)
            : base(b => b.MeetlrEventId == MeetlrEventId && 
                       b.Status == BookingStatus.Confirmed &&
                       b.StartTime < endTime && 
                       b.EndTime > startTime)
        {
        }
    }

    public class ConfirmedBookingsInTimeRange : BaseSpecification<Booking>
    {
        public ConfirmedBookingsInTimeRange(Guid MeetlrEventId, DateTime startTime, DateTime endTime)
            : base(b => b.MeetlrEventId == MeetlrEventId && 
                       b.Status == BookingStatus.Confirmed &&
                       b.StartTime < endTime && 
                       b.EndTime > startTime)
        {
        }
    }
}
