using Meetlr.Application.Features.Bookings.Queries.GetBookings;

namespace Meetlr.Api.Endpoints.Bookings.Get;

public class GetBookingsResponse
{
    public List<BookingItem> Bookings { get; set; } = new();

    public static GetBookingsResponse FromQueryResponse(GetBookingsQueryResponse queryResponse)
    {
        return new GetBookingsResponse
        {
            Bookings = queryResponse.Bookings.Select(b => new BookingItem
            {
                Id = b.Id,
                MeetlrEventId = b.MeetlrEventId,
                MeetlrEventName = b.meetlrEventName,
                InviteeName = b.InviteeName,
                InviteeEmail = b.InviteeEmail,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                Status = b.Status,
                MeetingLink = b.MeetingLink,
                Location = b.Location,
                CreatedAt = b.CreatedAt,
                SeriesBookingId = b.SeriesBookingId
            }).ToList()
        };
    }
}
