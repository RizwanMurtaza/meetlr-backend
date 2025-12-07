using MediatR;
using Meetlr.Api.Endpoints.Common;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.EventQuestions.Sync;

[Route("api/meetlr-events/{eventId}/questions")]
public class SyncEventQuestions : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public SyncEventQuestions(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Sync (create, update, delete) questions for a Meetlr event
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(SyncEventQuestionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle([FromRoute] Guid eventId, [FromBody] SyncEventQuestionsRequest request)
    {
        var command = SyncEventQuestionsRequest.ToCommand(eventId, request);
        var commandResponse = await _mediator.Send(command);
        var response = SyncEventQuestionsResponse.FromCommandResponse(commandResponse);

        return Ok(response);
    }
}
