using MediatR;
using Meetlr.Api.Endpoints.Common;
using Meetlr.Application.Features.MeetlrEvents.Queries.GetEventTypes;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.MeetlrEvents.Get;

[Route("api/MeetlrEvents")]
public class GetMeetlrEvents : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public GetMeetlrEvents(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(GetMeetlrEventsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle()
    {
        var userId = CurrentUserId;

        var query = new GetMeetlrEventsQuery
        {
            UserId = userId
        };

        var queryResponse = await _mediator.Send(query);
        var response = GetMeetlrEventsResponse.FromQueryResponse(queryResponse);

        return Ok(response);
    }
}
