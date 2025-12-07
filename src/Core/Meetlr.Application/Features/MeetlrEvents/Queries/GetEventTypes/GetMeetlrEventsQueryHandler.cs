using MediatR;
using Meetlr.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.MeetlrEvents.Queries.GetEventTypes;

public class GetMeetlrEventsQueryHandler : IRequestHandler<GetMeetlrEventsQuery, GetMeetlrEventsQueryResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMeetlrEventsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetMeetlrEventsQueryResponse> Handle(GetMeetlrEventsQuery request, CancellationToken cancellationToken)
    {
        var EventTypes = await _unitOfWork.Repository<Domain.Entities.Events.MeetlrEvent>().GetQueryable()
            .Include(e => e.Bookings)
            .Where(e => e.UserId == request.UserId && !e.IsDeleted)
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new MeetlrEventResponse
            {
                Id = e.Id,
                Name = e.Name,
                Slug = e.Slug,
                Description = e.Description,
                MeetingLocationType = e.MeetingLocationType,
                LocationDetails = e.LocationDetails,
                DurationMinutes = e.DurationMinutes,
                SlotIntervalMinutes = e.SlotIntervalMinutes,
                BufferTimeBeforeMinutes = e.BufferTimeBeforeMinutes,
                BufferTimeAfterMinutes = e.BufferTimeAfterMinutes,
                MinBookingNoticeMinutes = e.MinBookingNoticeMinutes,
                Color = e.Color,
                Status = e.Status,
                Fee = e.Fee,
                Currency = e.Currency,
                IsActive = e.IsActive,
                AvailabilityScheduleId = e.AvailabilityScheduleId,
                CreatedAt = e.CreatedAt,

                // Notification settings
                NotifyViaEmail = e.NotifyViaEmail,
                NotifyViaSms = e.NotifyViaSms,
                NotifyViaWhatsApp = e.NotifyViaWhatsApp,

                // Meeting type
                MeetingType = e.MeetingType,
                MaxAttendeesPerSlot = e.MaxAttendeesPerSlot,

                // Payment
                RequiresPayment = e.RequiresPayment,
                PaymentProviderType = e.PaymentProviderType != null ? e.PaymentProviderType.ToString() : null,

                // Recurring
                AllowsRecurring = e.AllowsRecurring,
                MaxRecurringOccurrences = e.MaxRecurringOccurrences,

                // Booking statistics
                TotalBookings = e.Bookings.Count(b => b.Status == Meetlr.Domain.Enums.BookingStatus.Confirmed),
                FutureBookings = e.Bookings.Count(b => b.Status == Meetlr.Domain.Enums.BookingStatus.Confirmed && b.StartTime > DateTime.UtcNow)
            })
            .ToListAsync(cancellationToken);

        return new GetMeetlrEventsQueryResponse
        {
            MeetlrEvents = EventTypes
        };
    }
}
