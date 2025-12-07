using Meetlr.Domain.Enums;

namespace Meetlr.Application.Features.Bookings.Queries.GetBookingSeries;

public record GetBookingSeriesQueryResponse
{
    public Guid Id { get; init; }
    public string InviteeName { get; init; } = string.Empty;
    public string InviteeEmail { get; init; } = string.Empty;
    public string? InviteeTimeZone { get; init; }
    public RecurrenceFrequency Frequency { get; init; }
    public int Interval { get; init; }
    public DayOfWeek? DayOfWeek { get; init; }
    public DateTime StartTime { get; init; }
    public int Duration { get; init; }
    public SeriesEndType EndType { get; init; }
    public DateTime? EndDate { get; init; }
    public int OccurrenceCount { get; init; }
    public int TotalOccurrences { get; init; }
    public SeriesPaymentType PaymentType { get; init; }
    public string? SubscriptionId { get; init; }
    public SeriesStatus Status { get; init; }
    public List<BookingOccurrenceDto> Occurrences { get; init; } = new();
    public DateTime? NextOccurrence { get; init; }
    public string meetlrEventName { get; init; } = string.Empty;
    public string HostName { get; init; } = string.Empty;
}
