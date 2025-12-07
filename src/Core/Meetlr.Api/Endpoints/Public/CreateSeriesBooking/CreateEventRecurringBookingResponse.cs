using Meetlr.Application.Features.Bookings.Commands.CreateRecurringBooking;

namespace Meetlr.Api.Endpoints.Public.CreateSeriesBooking;

public record CreateEventRecurringBookingResponse
{
    public bool HasConflicts { get; init; }
    public Guid? SeriesId { get; init; }
    public int TotalOccurrences { get; init; }
    public List<ConflictingOccurrenceDto>? ConflictingOccurrences { get; init; }
    public string? Message { get; init; }
    public bool RequiresPayment { get; init; }
    public decimal? TotalAmount { get; init; }
    public string? Currency { get; init; }
    public string? PaymentClientSecret { get; init; }
    public string? SubscriptionId { get; init; }

    public static CreateEventRecurringBookingResponse FromCommandResponse(CreateRecurringBookingCommandResponse response)
    {
        return new CreateEventRecurringBookingResponse
        {
            HasConflicts = response.HasConflicts,
            SeriesId = response.SeriesId,
            TotalOccurrences = response.TotalOccurrences,
            ConflictingOccurrences = response.ConflictingOccurrences?.Select(c => new ConflictingOccurrenceDto
            {
                OccurrenceNumber = c.OccurrenceNumber,
                RequestedDate = c.RequestedDate,
                RequestedTime = c.RequestedTime,
                SuggestedSlots = c.SuggestedSlots?.Select(s => new AlternativeSlotDto
                {
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    DisplayTime = s.DisplayTime
                }).ToList() ?? new()
            }).ToList(),
            Message = response.Message,
            RequiresPayment = response.RequiresPayment,
            TotalAmount = response.TotalAmount,
            Currency = response.Currency,
            PaymentClientSecret = response.PaymentClientSecret,
            SubscriptionId = response.SubscriptionId
        };
    }
}
