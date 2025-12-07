using MediatR;

namespace Meetlr.Module.Homepage.Application.Queries.GetPublicHomepage;

public record GetPublicHomepageQuery : IRequest<GetPublicHomepageResponse>
{
    public string Username { get; init; } = string.Empty;
}
