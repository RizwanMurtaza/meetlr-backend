using MediatR;
using Meetlr.Module.Board.Application.Commands.CreateTask;
using Meetlr.Module.Board.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Board.Endpoints.Tasks.CreateTask;

[Route("api/boards/{boardId:guid}/tasks")]
public class CreateTaskEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public CreateTaskEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new task in a column
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateTaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle(Guid boardId, [FromBody] CreateTaskRequest request)
    {
        var command = new CreateTaskCommand
        {
            BoardId = boardId,
            ColumnId = request.ColumnId,
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate,
            Priority = request.Priority,
            LabelIds = request.LabelIds
        };

        var commandResponse = await _mediator.Send(command);

        var response = new CreateTaskResponse
        {
            Id = commandResponse.Id,
            Title = commandResponse.Title,
            Description = commandResponse.Description,
            DueDate = commandResponse.DueDate,
            Priority = commandResponse.Priority,
            Position = commandResponse.Position,
            LabelIds = commandResponse.LabelIds
        };

        return Created($"/api/boards/{boardId}/tasks/{response.Id}", response);
    }
}
