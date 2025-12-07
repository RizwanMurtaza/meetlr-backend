namespace Meetlr.Application.Features.Bookings.Commands.CreateBooking;

/// <summary>
/// Response for create booking command
/// </summary>
public record CreateBookingCommandResponse
{
    public Guid BookingId { get; init; }
    public string ConfirmationToken { get; init; } = string.Empty;
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public string MeetlrEventName { get; init; } = string.Empty;
    public string HostName { get; init; } = string.Empty;
    public string? Location { get; init; }
    public string? MeetingLink { get; init; }
    public bool RequiresPayment { get; init; }
    public decimal? Amount { get; init; }
    public string? Currency { get; init; }
    public string? PaymentUrl { get; init; }
    public string? PaymentClientSecret { get; init; }
}
