using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Plugins.Payment.Stripe.Features.Queries.GetSeriesPaymentStatus;

public class GetSeriesPaymentStatusQueryHandler : IRequestHandler<GetSeriesPaymentStatusQuery, SeriesPaymentStatusResponse?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSeriesPaymentStatusQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SeriesPaymentStatusResponse?> Handle(GetSeriesPaymentStatusQuery request, CancellationToken cancellationToken)
    {
        var series = await _unitOfWork.Repository<BookingSeries>()
            .GetQueryable()
            .Include(s => s.BaseMeetlrEvent)
            .Include(s => s.HostUser)
            .Include(s => s.Contact)
            .Include(s => s.Bookings)
            .FirstOrDefaultAsync(s => s.Id == request.SeriesId, cancellationToken);

        if (series == null)
            return null;

        // Get payment status from the first booking (all bookings in a series share the same payment status)
        var firstBooking = series.Bookings.OrderBy(b => b.StartTime).FirstOrDefault();
        var paymentStatus = firstBooking?.PaymentStatus ?? PaymentStatus.Pending;
        var paidAt = firstBooking?.PaidAt;

        // Calculate total amount (fee per occurrence * total occurrences)
        decimal? totalAmount = null;
        string? currency = null;

        if (series.BaseMeetlrEvent.RequiresPayment)
        {
            totalAmount = series.BaseMeetlrEvent.Fee * series.TotalOccurrences;
            currency = series.BaseMeetlrEvent.Currency ?? "USD";
        }

        return new SeriesPaymentStatusResponse
        {
            SeriesId = series.Id,
            Status = series.Status.ToString(),
            PaymentStatus = paymentStatus.ToString(),
            RequiresPayment = series.BaseMeetlrEvent.RequiresPayment,
            TotalAmount = totalAmount,
            Currency = currency,
            PaidAt = paidAt,
            EventTypeName = series.BaseMeetlrEvent.Name,
            HostName = $"{series.HostUser.FirstName} {series.HostUser.LastName}",
            InviteeName = series.Contact?.Name ?? string.Empty,
            InviteeEmail = series.Contact?.Email ?? string.Empty,
            TotalOccurrences = series.TotalOccurrences,
            Occurrences = series.Bookings
                .OrderBy(b => b.StartTime)
                .Select(b => new OccurrenceInfo
                {
                    BookingId = b.Id,
                    StartTime = b.StartTime,
                    EndTime = b.EndTime,
                    Location = b.Location ?? string.Empty,
                    MeetingLink = b.MeetingLink
                })
                .ToList()
        };
    }
}
