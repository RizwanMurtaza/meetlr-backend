using MediatR;
using Meetlr.Application.Features.MeetlrEvents.Queries.GetBySlug;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Public.GetBySlug;

[ApiController]
[Route("api/public/meetlrEvents")]
[AllowAnonymous]
public class GetMeetlrEventBySlug : ControllerBase
{
    private readonly IMediator _mediator;

    public GetMeetlrEventBySlug(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("username/{username}/slug/{slug}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(GetMeetlrEventBySlugResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle(string username, string slug)
    {
        var query = new GetMeetlrEventBySlugQuery
        {
            Username = username,
            Slug = slug
        };

        var queryResponse = await _mediator.Send(query);
        var response = GetMeetlrEventBySlugResponse.FromQueryResponse(queryResponse);

        return Ok(response);
    }
}
