using MediatR;
using Meetlr.Application.Features.MeetlrEvents.Queries.GetTypesByUsername;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Public.GetByUsername;

[ApiController]
[Route("api/public")]
[AllowAnonymous]
public class GetMeetlrEventsByUsername : ControllerBase
{
    private readonly IMediator _mediator;

    public GetMeetlrEventsByUsername(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("username/{username}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(GetMeetlrEventsByUsernameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle(string username)
    {
        var query = new GetMeetlrEventsByUsernameQuery
        {
            Username = username
        };

        var queryResponse = await _mediator.Send(query);
        var response = GetMeetlrEventsByUsernameResponse.FromQueryResponse(queryResponse);

        return Ok(response);
    }
}
