using MediatR;
using Meetlr.Module.Homepage.Application.Commands.SaveUserHomepage;
using Meetlr.Module.Homepage.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Homepage.Endpoints.Save;

[Route("api/user-homepage")]
public class SaveUserHomepage : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public SaveUserHomepage(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Saves (creates or updates) the current user's homepage
    /// </summary>
    [HttpPut]
    [Produces("application/json")]
    [ProducesResponseType(typeof(SaveUserHomepageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle([FromBody] SaveUserHomepageRequest request)
    {
        var command = request.ToCommand(CurrentUserId);
        var response = await _mediator.Send(command);
        return Ok(response);
    }
}
