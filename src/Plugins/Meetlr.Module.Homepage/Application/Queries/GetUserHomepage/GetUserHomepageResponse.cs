namespace Meetlr.Module.Homepage.Application.Queries.GetUserHomepage;

public record GetUserHomepageResponse
{
    public UserHomepageDto? Homepage { get; init; }
    public bool HasHomepage { get; init; }
}
