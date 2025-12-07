using MediatR;

namespace Meetlr.Application.Features.Availability.Queries.GetAvailableSlots;

/// <summary>
/// Query to get available time slots for an event type
/// </summary>
public record GetAvailableSlotsQuery : IRequest<GetAvailableSlotsQueryResponse>
{
    public Guid MeetlrEventId { get; init; }
    public string? UserSlug { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string? TimeZone { get; init; }
}
