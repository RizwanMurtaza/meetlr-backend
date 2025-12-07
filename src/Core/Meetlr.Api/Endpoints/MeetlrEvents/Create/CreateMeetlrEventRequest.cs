using Meetlr.Application.Features.MeetlrEvents.Commands.Create;
using Meetlr.Domain.Enums;

namespace Meetlr.Api.Endpoints.MeetlrEvents.Create;

public class CreateMeetlrEventRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public MeetingLocationType MeetingLocationType { get; init; } = MeetingLocationType.InPerson;
    public string? LocationDetails { get; init; }
    public int DurationMinutes { get; init; } = 30;
    public int SlotIntervalMinutes { get; init; } = 15;
    public int BufferTimeBeforeMinutes { get; init; } = 0;
    public int BufferTimeAfterMinutes { get; init; } = 0;
    public string Color { get; init; } = "#0069ff";
    public string? SlugUrl { get; init; }
    public int MinBookingNoticeMinutes { get; init; } = 60;
    public int MaxBookingDaysInFuture { get; init; } = 60;
    public bool RequiresPayment { get; init; }
    public decimal? Fee { get; init; }
    public string Currency { get; init; } = "USD";
    public string? PaymentProviderType { get; init; } // e.g., "stripe", "paypal"
    public Guid? AvailabilityScheduleId { get; init; }
    public MeetingType MeetingType { get; init; } = MeetingType.OneOnOne;
    public int? MaxAttendeesPerSlot { get; init; }
    public List<MeetlrEventQuestionRequest>? Questions { get; init; }
    public bool AllowsRecurring { get; init; } = false;
    public int? MaxRecurringOccurrences { get; init; } = 10;

    public bool NotifyViaEmail { get; init; } = true;     
    
    public bool NotifyViaSms { get; init; } = false;
    
    public bool NotifyViaWhatsApp { get; init; } = false;
    
    public static CreateMeetlrEventCommand ToCommand(CreateMeetlrEventRequest request)
    {
        return new CreateMeetlrEventCommand
        {
            Name = request.Name,
            Description = request.Description,
            MeetingLocationType = request.MeetingLocationType,
            LocationDetails = request.LocationDetails,
            DurationMinutes = request.DurationMinutes,
            SlotIntervalMinutes = request.SlotIntervalMinutes,
            BufferTimeBeforeMinutes = request.BufferTimeBeforeMinutes,
            BufferTimeAfterMinutes = request.BufferTimeAfterMinutes,
            Color = request.Color,
            SlugUrl = request.SlugUrl,
            MinBookingNoticeMinutes = request.MinBookingNoticeMinutes,
            MaxBookingDaysInFuture = request.MaxBookingDaysInFuture,
            RequiresPayment = request.RequiresPayment,
            Fee = request.Fee,
            Currency = request.Currency,
            PaymentProviderType = !string.IsNullOrEmpty(request.PaymentProviderType)
                ? Enum.Parse<PaymentProviderType>(request.PaymentProviderType, ignoreCase: true)
                : null,
            AvailabilityScheduleId = request.AvailabilityScheduleId,
            MeetingType = request.MeetingType,
            MaxAttendeesPerSlot = request.MaxAttendeesPerSlot,
            AllowsRecurring = request.AllowsRecurring,
            MaxRecurringOccurrences = request.MaxRecurringOccurrences,
            NotifyViaEmail = request.NotifyViaEmail,
            NotifyViaSms = request.NotifyViaSms,
            NotifyViaWhatsApp = request.NotifyViaWhatsApp,
            Questions = request.Questions?.Select(q => new CreateMeetlrEventQuestionRequest
            {
                QuestionText = q.QuestionText,
                Type = q.Type,
                IsRequired = q.IsRequired,
                DisplayOrder = q.DisplayOrder,
                Options = q.Options
            }).ToList()
        };
    }
}
