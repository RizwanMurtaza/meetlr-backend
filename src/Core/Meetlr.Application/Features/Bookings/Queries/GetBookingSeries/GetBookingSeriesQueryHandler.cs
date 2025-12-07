using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Bookings.Queries.GetBookingSeries;

public class GetBookingSeriesQueryHandler : IRequestHandler<GetBookingSeriesQuery, GetBookingSeriesQueryResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBookingSeriesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetBookingSeriesQueryResponse> Handle(
        GetBookingSeriesQuery request,
        CancellationToken cancellationToken)
    {
        // Load series with BaseMeetlrEvent, User and Contact in single query (fixes N+1)
        var series = await _unitOfWork.Repository<BookingSeries>()
            .GetQueryable()
            .AsNoTracking()
            .Include(s => s.BaseMeetlrEvent)
                .ThenInclude(et => et.User)
            .Include(s => s.Contact)
            .FirstOrDefaultAsync(s => s.Id == request.SeriesId, cancellationToken)
            .ConfigureAwait(false);

        if (series == null)
            throw BookingErrors.SeriesNotFound(request.SeriesId);

        if (series.BaseMeetlrEvent == null)
            throw MeetlrEventErrors.MeetlrEventNotFound(series.BaseMeetlrEventId);

        // Get all bookings for this series async (fixes synchronous ToList)
        var bookings = await _unitOfWork.Repository<Booking>()
            .GetQueryable()
            .AsNoTracking()
            .Where(b => b.SeriesBookingId == series.Id && !b.IsDeleted)
            .OrderBy(b => b.StartTime)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var occurrences = bookings.Select((b, index) => new BookingOccurrenceDto
        {
            Id = b.Id,
            OccurrenceNumber = index + 1,
            StartTime = b.StartTime,
            EndTime = b.EndTime,
            Status = b.Status,
            PaymentStatus = b.PaymentStatus,
            Location = b.Location,
            MeetingLink = b.MeetingLink,
            HasCalendarEvent = !string.IsNullOrEmpty(b.CalendarEventId)
        }).ToList();

        var nextOccurrence = bookings
            .Where(b => b.StartTime > DateTime.UtcNow && b.Status == Meetlr.Domain.Enums.BookingStatus.Confirmed)
            .OrderBy(b => b.StartTime)
            .FirstOrDefault()
            ?.StartTime;

        // Note: Series model simplified - frequency/interval/etc no longer tracked
        // These fields are set to defaults for backward compatibility
        return new GetBookingSeriesQueryResponse
        {
            Id = series.Id,
            InviteeName = series.Contact?.Name ?? string.Empty,
            InviteeEmail = series.Contact?.Email ?? string.Empty,
            InviteeTimeZone = series.Contact?.TimeZone, // From Contact
            Frequency = Meetlr.Domain.Enums.RecurrenceFrequency.Weekly, // Default
            Interval = 1, // Default
            DayOfWeek = null, // No longer applicable
            StartTime = bookings.FirstOrDefault()?.StartTime ?? DateTime.UtcNow, // Use first booking's time
            Duration = bookings.FirstOrDefault() != null
                ? (int)(bookings.First().EndTime - bookings.First().StartTime).TotalMinutes
                : 60, // Default
            EndType = Meetlr.Domain.Enums.SeriesEndType.AfterOccurrences, // Default
            EndDate = null, // No longer tracked
            OccurrenceCount = series.OccurrenceCount,
            TotalOccurrences = series.TotalOccurrences,
            PaymentType = series.PaymentType,
            SubscriptionId = series.SubscriptionId,
            Status = series.Status,
            Occurrences = occurrences,
            NextOccurrence = nextOccurrence,
            meetlrEventName = series.BaseMeetlrEvent.Name,
            HostName = $"{series.BaseMeetlrEvent.User.FirstName} {series.BaseMeetlrEvent.User.LastName}"
        };
    }
}
