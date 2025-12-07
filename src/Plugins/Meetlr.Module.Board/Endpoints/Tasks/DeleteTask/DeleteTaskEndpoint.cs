using MediatR;
using Meetlr.Module.Board.Application.Commands.DeleteTask;
using Meetlr.Module.Board.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Board.Endpoints.Tasks.DeleteTask;

[Route("api/boards/{boardId:guid}/tasks")]
public class DeleteTaskEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public DeleteTaskEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Delete a task
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle(Guid boardId, Guid id)
    {
        var command = new DeleteTaskCommand
        {
            BoardId = boardId,
            Id = id
        };
        await _mediator.Send(command);
        return NoContent();
    }
}
