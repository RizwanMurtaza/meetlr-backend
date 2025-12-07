using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Plugins.Payment.Stripe.Features.Queries.GetBookingPaymentStatus;

public class GetBookingPaymentStatusQueryHandler : IRequestHandler<GetBookingPaymentStatusQuery, BookingPaymentStatusResponse?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBookingPaymentStatusQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BookingPaymentStatusResponse?> Handle(GetBookingPaymentStatusQuery request, CancellationToken cancellationToken)
    {
        var booking = await _unitOfWork.Repository<Booking>()
            .GetQueryable()
            .Include(b => b.MeetlrEvent)
            .Include(b => b.HostUser)
            .Include(b => b.Contact)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);

        if (booking == null)
            return null;

        return new BookingPaymentStatusResponse
        {
            BookingId = booking.Id,
            Status = booking.Status.ToString(),
            PaymentStatus = booking.PaymentStatus.ToString(),
            RequiresPayment = booking.MeetlrEvent.RequiresPayment,
            Amount = booking.Amount,
            Currency = booking.Currency,
            PaidAt = booking.PaidAt,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            EventTypeName = booking.MeetlrEvent.Name,
            HostName = $"{booking.HostUser.FirstName} {booking.HostUser.LastName}",
            InviteeName = booking.Contact?.Name ?? string.Empty,
            InviteeEmail = booking.Contact?.Email ?? string.Empty,
            Location = booking.Location,
            MeetingLink = booking.MeetingLink
        };
    }
}
