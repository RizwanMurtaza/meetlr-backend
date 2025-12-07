using Meetlr.Domain.Enums;

namespace Meetlr.Application.Features.Availability.Queries.GetUserBookingSeries;

public record BookingSeriesSummaryDto
{
    public Guid Id { get; init; }
    public string InviteeName { get; init; } = string.Empty;
    public string InviteeEmail { get; init; } = string.Empty;
    public RecurrenceFrequency Frequency { get; init; }
    public SeriesStatus Status { get; init; }
    public int OccurrenceCount { get; init; }
    public int TotalOccurrences { get; init; }
    public DateTime? NextOccurrence { get; init; }
    public DateTime CreatedAt { get; init; }
    public string meetlrEventName { get; init; } = string.Empty;
    public SeriesPaymentType PaymentType { get; init; }
}
