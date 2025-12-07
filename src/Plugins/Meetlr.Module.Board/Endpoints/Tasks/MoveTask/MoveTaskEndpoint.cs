using MediatR;
using Meetlr.Module.Board.Application.Commands.MoveTask;
using Meetlr.Module.Board.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Board.Endpoints.Tasks.MoveTask;

[Route("api/boards/{boardId:guid}/tasks")]
public class MoveTaskEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public MoveTaskEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Move a task to a different column or position
    /// </summary>
    [HttpPut("{id:guid}/move")]
    [ProducesResponseType(typeof(MoveTaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle(Guid boardId, Guid id, [FromBody] MoveTaskRequest request)
    {
        var command = new MoveTaskCommand
        {
            BoardId = boardId,
            TaskId = id,
            TargetColumnId = request.TargetColumnId,
            NewPosition = request.NewPosition
        };

        await _mediator.Send(command);
        return Ok(new MoveTaskResponse { Success = true });
    }
}
