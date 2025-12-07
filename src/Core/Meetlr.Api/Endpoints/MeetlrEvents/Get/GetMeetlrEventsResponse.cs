using Meetlr.Application.Features.MeetlrEvents.Queries.GetEventTypes;

namespace Meetlr.Api.Endpoints.MeetlrEvents.Get;

public class GetMeetlrEventsResponse
{
    public List<MeetlrEventItem> MeetlrEvents { get; set; } = new();

    public static GetMeetlrEventsResponse FromQueryResponse(GetMeetlrEventsQueryResponse queryResponse)
    {
        return new GetMeetlrEventsResponse
        {
            MeetlrEvents = queryResponse.MeetlrEvents.Select(e => new MeetlrEventItem
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
                PaymentProviderType = e.PaymentProviderType,

                // Recurring
                AllowsRecurring = e.AllowsRecurring,
                MaxRecurringOccurrences = e.MaxRecurringOccurrences,

                // Booking statistics
                TotalBookings = e.TotalBookings,
                FutureBookings = e.FutureBookings
            }).ToList()
        };
    }
}
