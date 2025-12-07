using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Availability.Queries.GetUserBookingSeries;

public class GetUserBookingSeriesQueryHandler : IRequestHandler<GetUserBookingSeriesQuery, GetUserBookingSeriesQueryResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserBookingSeriesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetUserBookingSeriesQueryResponse> Handle(
        GetUserBookingSeriesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<BookingSeries>()
            .GetQueryable()
            .AsNoTracking()
            .Where(s => s.HostUserId == request.UserId && !s.IsDeleted);

        // Apply filters
        if (request.StatusFilter.HasValue)
        {
            query = query.Where(s => s.Status == request.StatusFilter.Value);
        }

        if (request.PaymentTypeFilter.HasValue)
        {
            query = query.Where(s => s.PaymentType == request.PaymentTypeFilter.Value);
        }

        // Use async count
        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        // Get series IDs first for batch loading
        var seriesIds = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!seriesIds.Any())
        {
            return new GetUserBookingSeriesQueryResponse
            {
                Series = new List<BookingSeriesSummaryDto>(),
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        // Batch load series with BaseMeetlrEvent and Contact in single query (fixes N+1)
        var series = await _unitOfWork.Repository<BookingSeries>()
            .GetQueryable()
            .AsNoTracking()
            .Include(s => s.BaseMeetlrEvent)
            .Include(s => s.Contact)
            .Where(s => seriesIds.Contains(s.Id))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Batch load next occurrences for all series in single query (fixes N+1)
        var now = DateTime.UtcNow;
        var nextOccurrences = await _unitOfWork.Repository<Booking>()
            .GetQueryable()
            .AsNoTracking()
            .Where(b => seriesIds.Contains(b.SeriesBookingId!.Value)
                    && !b.IsDeleted
                    && b.StartTime > now)
            .GroupBy(b => b.SeriesBookingId!.Value)
            .Select(g => new
            {
                SeriesId = g.Key,
                NextOccurrence = g.OrderBy(b => b.StartTime).First().StartTime
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Create lookup dictionary for O(1) access
        var nextOccurrenceLookup = nextOccurrences.ToDictionary(
            x => x.SeriesId,
            x => (DateTime?)x.NextOccurrence);

        // Create lookup dictionary to preserve original order
        var seriesLookup = series.ToDictionary(s => s.Id);

        // Map results in original order
        var result = seriesIds
            .Select(id => seriesLookup.TryGetValue(id, out var s) ? s : null)
            .Where(s => s != null)
            .Select(s => new BookingSeriesSummaryDto
            {
                Id = s!.Id,
                InviteeName = s.Contact?.Name ?? string.Empty,
                InviteeEmail = s.Contact?.Email ?? string.Empty,
                Frequency = RecurrenceFrequency.Weekly, // Default value - series no longer track frequency
                Status = s.Status,
                OccurrenceCount = s.OccurrenceCount,
                TotalOccurrences = s.TotalOccurrences,
                NextOccurrence = nextOccurrenceLookup.GetValueOrDefault(s.Id),
                CreatedAt = s.CreatedAt,
                meetlrEventName = s.BaseMeetlrEvent?.Name ?? "Unknown",
                PaymentType = s.PaymentType
            })
            .ToList();

        return new GetUserBookingSeriesQueryResponse
        {
            Series = result,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
