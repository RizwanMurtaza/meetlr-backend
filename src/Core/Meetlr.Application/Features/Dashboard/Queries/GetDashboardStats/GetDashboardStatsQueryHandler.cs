using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Dashboard.Queries.GetDashboardStats;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, GetDashboardStatsQueryResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDashboardStatsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetDashboardStatsQueryResponse> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        // Count active event types for the user
        var eventTypesCount = await _unitOfWork.Repository<MeetlrEvent>()
            .GetQueryable()
            .Where(e => e.UserId == request.UserId && e.IsActive)
            .CountAsync(cancellationToken);

        // Count upcoming bookings (confirmed bookings with start time in the future)
        var now = DateTime.UtcNow;
        var upcomingBookingsCount = await _unitOfWork.Repository<Booking>()
            .GetQueryable()
            .Where(b => b.HostUserId == request.UserId
                && b.StartTime > now
                && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending))
            .CountAsync(cancellationToken);

        // Count total bookings for the user (all statuses)
        var totalBookingsCount = await _unitOfWork.Repository<Booking>()
            .GetQueryable()
            .Where(b => b.HostUserId == request.UserId)
            .CountAsync(cancellationToken);

        return new GetDashboardStatsQueryResponse
        {
            EventTypesCount = eventTypesCount,
            UpcomingBookingsCount = upcomingBookingsCount,
            TotalBookingsCount = totalBookingsCount
        };
    }
}
