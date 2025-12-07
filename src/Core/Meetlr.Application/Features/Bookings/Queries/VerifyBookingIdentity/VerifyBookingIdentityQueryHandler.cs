using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Bookings.Queries.VerifyBookingIdentity;

public class VerifyBookingIdentityQueryHandler : IRequestHandler<VerifyBookingIdentityQuery, VerifyBookingIdentityQueryResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public VerifyBookingIdentityQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<VerifyBookingIdentityQueryResponse> Handle(
        VerifyBookingIdentityQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Get booking with confirmation token
        var booking = await _unitOfWork.Repository<Booking>()
            .GetQueryable()
            .Include(b => b.Contact)
            .Include(b => b.MeetlrEvent)
            .Include(b => b.HostUser)
            .FirstOrDefaultAsync(b =>
                b.Id == request.BookingId &&
                b.ConfirmationToken == request.ConfirmationToken,
                cancellationToken);

        if (booking == null)
        {
            throw BookingErrors.BookingNotFound(request.BookingId, "Booking not found or invalid confirmation token");
        }

        // 2. Verify identity (email or phone must match)
        var identifier = request.Identifier.Trim().ToLowerInvariant();
        var contactEmail = booking.Contact?.Email?.ToLowerInvariant() ?? string.Empty;
        var contactPhone = booking.Contact?.Phone?.Replace(" ", "").Replace("-", "").Replace("+", "") ?? string.Empty;
        var normalizedIdentifier = identifier.Replace(" ", "").Replace("-", "").Replace("+", "");

        var isEmailMatch = !string.IsNullOrEmpty(contactEmail) && contactEmail == identifier;
        var isPhoneMatch = !string.IsNullOrEmpty(contactPhone) && contactPhone == normalizedIdentifier;

        if (!isEmailMatch && !isPhoneMatch)
        {
            throw BookingErrors.InvalidIdentityVerification();
        }

        // 3. Check if booking can be rescheduled
        var canReschedule = true;
        string? blockedReason = null;

        // Check status
        if (booking.Status != BookingStatus.Confirmed)
        {
            canReschedule = false;
            blockedReason = $"Booking status is {booking.Status}. Only confirmed bookings can be rescheduled.";
        }
        // Check reschedule limit
        else if (booking.RescheduleCount >= 1)
        {
            canReschedule = false;
            blockedReason = "This booking has already been rescheduled once.";
        }
        // Check 72-hour rule
        else if (booking.StartTime <= DateTime.UtcNow.AddHours(72))
        {
            canReschedule = false;
            blockedReason = "Bookings cannot be rescheduled within 72 hours of the scheduled time.";
        }

        // 4. Return response
        return new VerifyBookingIdentityQueryResponse
        {
            Success = true,
            CanReschedule = canReschedule,
            BlockedReason = blockedReason,
            BookingId = booking.Id,
            CurrentStartTime = booking.StartTime,
            CurrentEndTime = booking.EndTime,
            RescheduleCount = booking.RescheduleCount,
            MeetlrEventId = booking.MeetlrEventId,
            EventName = booking.MeetlrEvent?.Name ?? string.Empty,
            DurationMinutes = booking.MeetlrEvent?.DurationMinutes ?? 30,
            HostName = $"{booking.HostUser?.FirstName} {booking.HostUser?.LastName}".Trim(),
            HostUsername = booking.HostUser?.MeetlrUsername ?? string.Empty,
            GuestName = booking.Contact?.Name ?? string.Empty,
            GuestEmail = booking.Contact?.Email ?? string.Empty
        };
    }
}
