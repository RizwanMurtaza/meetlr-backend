using MediatR;
using Meetlr.Module.Board.Application.Commands.UpdateTask;
using Meetlr.Module.Board.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Board.Endpoints.Tasks.UpdateTask;

[Route("api/boards/{boardId:guid}/tasks")]
public class UpdateTaskEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public UpdateTaskEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Update a task
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UpdateTaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle(Guid boardId, Guid id, [FromBody] UpdateTaskRequest request)
    {
        var command = new UpdateTaskCommand
        {
            BoardId = boardId,
            Id = id,
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate,
            Priority = request.Priority,
            LabelIds = request.LabelIds
        };

        var commandResponse = await _mediator.Send(command);

        var response = new UpdateTaskResponse
        {
            Id = commandResponse.Id,
            Title = commandResponse.Title,
            Description = commandResponse.Description,
            DueDate = commandResponse.DueDate,
            Priority = commandResponse.Priority,
            LabelIds = commandResponse.LabelIds
        };

        return Ok(response);
    }
}
