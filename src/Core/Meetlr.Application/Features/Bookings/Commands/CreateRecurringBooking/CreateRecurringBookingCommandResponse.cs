namespace Meetlr.Application.Features.Bookings.Commands.CreateRecurringBooking;

public record CreateRecurringBookingCommandResponse
{
    public bool HasConflicts { get; init; }
    public Guid? SeriesId { get; init; }
    public int TotalOccurrences { get; init; }
    public List<ConflictingOccurrence>? ConflictingOccurrences { get; init; }
    public string? Message { get; init; }
    public decimal? TotalAmount { get; init; }
    public string? Currency { get; init; }
    public bool RequiresPayment { get; init; }
    public string? PaymentClientSecret { get; init; }
    public string? SubscriptionId { get; init; }
}
