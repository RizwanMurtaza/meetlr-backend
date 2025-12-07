using Meetlr.Domain.Enums;

namespace Meetlr.Application.Features.Bookings.Queries.GetBookingSeries;

public record BookingOccurrenceDto
{
    public Guid Id { get; init; }
    public int OccurrenceNumber { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public BookingStatus Status { get; init; }
    public PaymentStatus PaymentStatus { get; init; }
    public string? Location { get; init; }
    public string? MeetingLink { get; init; }
    public bool HasCalendarEvent { get; init; }
}
