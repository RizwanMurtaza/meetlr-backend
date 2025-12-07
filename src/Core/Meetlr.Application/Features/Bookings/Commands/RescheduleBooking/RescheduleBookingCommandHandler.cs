using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Features.Availability.Queries.ValidateBookingSlots;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Application.Features.Bookings.Commands.RescheduleBooking;

public class RescheduleBookingCommandHandler : IRequestHandler<RescheduleBookingCommand, RescheduleBookingCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<RescheduleBookingCommandHandler> _logger;

    public RescheduleBookingCommandHandler(
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger<RescheduleBookingCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<RescheduleBookingCommandResponse> Handle(
        RescheduleBookingCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing reschedule request for booking {BookingId}",
            request.BookingId);

        // 1. Get booking with confirmation token
        var booking = await _unitOfWork.Repository<Booking>()
            .GetQueryable()
            .Include(b => b.Contact)
            .Include(b => b.MeetlrEvent)
            .FirstOrDefaultAsync(b =>
                b.Id == request.BookingId &&
                b.ConfirmationToken == request.ConfirmationToken,
                cancellationToken);

        if (booking == null)
        {
            throw BookingErrors.BookingNotFound(request.BookingId, "Booking not found or invalid confirmation token");
        }

        // 2. Re-verify identity (email or phone must match)
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

        // 3. Validate slot availability (exclude current booking from conflict check)
        var validateSlotsQuery = new ValidateBookingSlotsQuery
        {
            MeetlrEventId = booking.MeetlrEventId,
            RequestedSlots = new List<DateTime> { request.NewStartTime },
            TimeZone = booking.Contact?.TimeZone ?? "UTC"
        };

        var validationResult = await _mediator.Send(validateSlotsQuery, cancellationToken);

        // Check if the conflict is with the same booking being rescheduled
        if (validationResult.HasConflicts)
        {
            // Get existing bookings at the new time
            var conflictingBookings = await _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .Where(b =>
                    b.MeetlrEventId == booking.MeetlrEventId &&
                    b.Id != booking.Id && // Exclude current booking
                    b.Status != BookingStatus.Cancelled &&
                    b.StartTime < request.NewEndTime &&
                    b.EndTime > request.NewStartTime)
                .AnyAsync(cancellationToken);

            if (conflictingBookings)
            {
                throw BookingErrors.TimeSlotConflict(request.NewStartTime, request.NewEndTime,
                    "The selected time slot is not available");
            }
        }

        // 4. Call Reschedule() on the booking entity
        // This validates 72-hour rule, 1 reschedule limit, and raises BookingRescheduledEvent
        booking.Reschedule(request.NewStartTime, request.NewEndTime);

        // 5. Save changes (domain event will be published)
        _unitOfWork.Repository<Booking>().Update(booking);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Booking {BookingId} rescheduled from {OldStartTime} to {NewStartTime}",
            booking.Id,
            request.NewStartTime,
            request.NewEndTime);

        return new RescheduleBookingCommandResponse
        {
            Success = true,
            NewStartTime = booking.StartTime,
            NewEndTime = booking.EndTime,
            RescheduleCount = booking.RescheduleCount
        };
    }
}
