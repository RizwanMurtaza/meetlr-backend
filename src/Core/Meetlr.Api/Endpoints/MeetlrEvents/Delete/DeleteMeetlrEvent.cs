using MediatR;
using Meetlr.Api.Endpoints.Common;
using Meetlr.Application.Features.MeetlrEvents.Commands.Delete;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.MeetlrEvents.Delete;

[Route("api/MeetlrEvents")]
public class DeleteMeetlrEvent : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public DeleteMeetlrEvent(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpDelete("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(DeleteMeetlrEventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle(Guid id)
    {
        var userId = CurrentUserId;

        var command = new DeleteMeetlrEventCommand
        {
            Id = id,
            UserId = userId
        };

        var commandResponse = await _mediator.Send(command);
        var response = DeleteMeetlrEventResponse.FromCommandResponse(commandResponse);

        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }
}
