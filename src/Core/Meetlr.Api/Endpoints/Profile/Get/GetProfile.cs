using MediatR;
using Meetlr.Api.Endpoints.Common;
using Meetlr.Application.Features.Profile.Queries.GetProfile;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Profile.Get;

[Route("api/profile")]
public class GetProfile : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public GetProfile(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(GetProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle()
    {
        var userId = CurrentUserId;

        var query = new GetProfileQuery
        {
            UserId = userId
        };

        var queryResponse = await _mediator.Send(query);
        var response = GetProfileResponse.FromQueryResponse(queryResponse);

        return Ok(response);
    }
}
