using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Bookings.Queries.GetBookings;

public class GetBookingsQueryHandler : IRequestHandler<GetBookingsQuery, GetBookingsQueryResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBookingsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetBookingsQueryResponse> Handle(GetBookingsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Booking>().GetQueryable()
            .Include(b => b.MeetlrEvent)
            .Include(b => b.Contact)
            .Where(b => b.HostUserId == request.UserId);

        if (request.StartDate.HasValue)
        {
            query = query.Where(b => b.StartTime >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(b => b.StartTime <= request.EndDate.Value);
        }

        var bookings = await query
            .OrderBy(b => b.StartTime)
            .Select(b => new BookingDto
            {
                Id = b.Id,
                MeetlrEventId = b.MeetlrEventId,
                meetlrEventName = b.MeetlrEvent.Name,
                // Get invitee info from Contact entity
                InviteeName = b.Contact != null ? b.Contact.Name : string.Empty,
                InviteeEmail = b.Contact != null ? b.Contact.Email : string.Empty,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                Status = b.Status,
                MeetingLink = b.MeetingLink,
                Location = b.Location,
                CreatedAt = b.CreatedAt,
                SeriesBookingId = b.SeriesBookingId
            })
            .ToListAsync(cancellationToken);

        return new GetBookingsQueryResponse
        {
            Bookings = bookings
        };
    }
}
