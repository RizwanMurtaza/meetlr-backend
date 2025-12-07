namespace Meetlr.Application.Features.MeetlrEvents.Queries.GetTypesByUsername;

public record HostInfo
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? CompanyName { get; init; }
    public string? LogoUrl { get; init; }
}