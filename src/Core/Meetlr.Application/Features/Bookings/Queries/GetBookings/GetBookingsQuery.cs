using MediatR;

namespace Meetlr.Application.Features.Bookings.Queries.GetBookings;

public class GetBookingsQuery : IRequest<GetBookingsQueryResponse>
{
    public Guid UserId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
