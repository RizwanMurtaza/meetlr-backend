namespace Meetlr.Module.Homepage.Application.Queries.GetPublicHomepage;

public record GetPublicHomepageResponse
{
    public bool Found { get; init; }
    public PublicHomepageDto? Homepage { get; init; }
}
