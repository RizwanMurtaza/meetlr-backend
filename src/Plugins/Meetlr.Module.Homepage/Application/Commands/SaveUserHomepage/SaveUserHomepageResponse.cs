namespace Meetlr.Module.Homepage.Application.Commands.SaveUserHomepage;

public record SaveUserHomepageResponse
{
    public Guid HomepageId { get; init; }
    public string Username { get; init; } = string.Empty;
    public bool IsPublished { get; init; }
    public string Message { get; init; } = string.Empty;
    public string PublicUrl { get; init; } = string.Empty;
    public string SubdomainUrl { get; init; } = string.Empty;
}
