using MediatR;
using Meetlr.Module.Homepage.Application.Queries.GetUserHomepage;
using Meetlr.Module.Homepage.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Homepage.Endpoints.Get;

[Route("api/user-homepage")]
public class GetUserHomepage : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public GetUserHomepage(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets the current user's homepage configuration
    /// </summary>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(GetUserHomepageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle()
    {
        var query = new GetUserHomepageQuery
        {
            UserId = CurrentUserId
        };

        var response = await _mediator.Send(query);
        return Ok(response);
    }
}
