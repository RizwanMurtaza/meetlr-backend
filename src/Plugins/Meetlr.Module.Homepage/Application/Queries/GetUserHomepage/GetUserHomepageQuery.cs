using MediatR;

namespace Meetlr.Module.Homepage.Application.Queries.GetUserHomepage;

public record GetUserHomepageQuery : IRequest<GetUserHomepageResponse>
{
    public Guid UserId { get; init; }
}
