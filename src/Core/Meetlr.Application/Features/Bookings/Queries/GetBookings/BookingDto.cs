using Meetlr.Domain.Enums;

namespace Meetlr.Application.Features.Bookings.Queries.GetBookings;

public class BookingDto
{
    public Guid Id { get; set; }
    public Guid MeetlrEventId { get; set; }
    public string meetlrEventName { get; set; } = string.Empty;
    public string InviteeName { get; set; } = string.Empty;
    public string InviteeEmail { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public BookingStatus Status { get; set; }
    public string? MeetingLink { get; set; }
    public string? Location { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? SeriesBookingId { get; set; }
}
