using MediatR;
using Meetlr.Domain.Enums;

namespace Meetlr.Application.Features.MeetlrEvents.Commands.Create;

/// <summary>
/// Command to create a new event type
/// </summary>
public record CreateMeetlrEventCommand : IRequest<CreateMeetlrEventCommandResponse>
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
    public PaymentProviderType? PaymentProviderType { get; init; }
    public Guid? AvailabilityScheduleId { get; init; }
    public MeetingType MeetingType { get; init; } = MeetingType.OneOnOne;
    public int? MaxAttendeesPerSlot { get; init; }
    public List<CreateMeetlrEventQuestionRequest>? Questions { get; init; }
    public bool AllowsRecurring { get; init; } = false;
    public int? MaxRecurringOccurrences { get; init; } = 10;
    
    public bool NotifyViaEmail { get; init; } = true;     
    
    public bool NotifyViaSms { get; init; } = false;
    
    public bool NotifyViaWhatsApp { get; init; } = false;
}
