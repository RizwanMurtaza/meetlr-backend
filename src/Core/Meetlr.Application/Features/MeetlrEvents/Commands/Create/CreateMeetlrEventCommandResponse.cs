namespace Meetlr.Application.Features.MeetlrEvents.Commands.Create;

/// <summary>
/// Response for create event type command
/// </summary>
public record CreateMeetlrEventCommandResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int DurationMinutes { get; init; }
    public string? SlugUrl { get; init; }
    public string BookingUrl { get; init; } = string.Empty;
    public bool RequiresPayment { get; init; }
    public decimal? Fee { get; init; }
    public string? Currency { get; init; }
    public DateTime CreatedAt { get; init; }
}
