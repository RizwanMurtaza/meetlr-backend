namespace Meetlr.Application.Features.MeetlrEvents.Queries.GetTypesByUsername;

public record MeetlrEventListItem
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int DurationMinutes { get; init; }
    public string? Color { get; init; }
    public string? Location { get; init; }
    public decimal? Fee { get; init; }
    public string? Currency { get; init; }
    public bool AllowsRecurring { get; init; }
    public int? MaxRecurringOccurrences { get; init; }
    public bool IsActive { get; init; }
}