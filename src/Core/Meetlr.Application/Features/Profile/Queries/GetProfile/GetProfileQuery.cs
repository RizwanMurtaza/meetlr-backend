using MediatR;

namespace Meetlr.Application.Features.Profile.Queries.GetProfile;

public class GetProfileQuery : IRequest<GetProfileQueryResponse>
{
    public Guid UserId { get; set; }
}
