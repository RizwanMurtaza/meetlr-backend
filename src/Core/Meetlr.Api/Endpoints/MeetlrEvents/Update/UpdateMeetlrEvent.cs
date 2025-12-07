using MediatR;
using Meetlr.Api.Endpoints.Common;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.MeetlrEvents.Update;

[Route("api/MeetlrEvents")]
public class UpdateMeetlrEvent : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public UpdateMeetlrEvent(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPatch("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UpdateMeetlrEventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle(Guid id, [FromBody] UpdateMeetlrEventRequest request)
    {
        var userId = CurrentUserId;

        var command = UpdateMeetlrEventRequest.ToCommand(request, id, userId);
        var commandResponse = await _mediator.Send(command);
        var response = UpdateMeetlrEventResponse.FromCommandResponse(commandResponse);

        return Ok(response);
    }
}
