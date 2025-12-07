using MediatR;
using Meetlr.Module.Board.Application.Queries.GetBoardById;
using Meetlr.Module.Board.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Board.Endpoints.Boards.GetBoardById;

[Route("api/boards")]
public class GetBoardByIdEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public GetBoardByIdEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get a board by ID with all columns, tasks, and labels
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetBoardByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle(Guid id)
    {
        var query = new GetBoardByIdQuery { Id = id };
        var queryResponse = await _mediator.Send(query);

        var board = queryResponse.Board!;
        var response = new GetBoardByIdResponse
        {
            Id = board.Id,
            Name = board.Name,
            Description = board.Description,
            Color = board.Color ?? "#6366f1",
            Position = board.Position,
            CreatedAt = board.CreatedAt,
            Columns = board.Columns.Select(c => new ColumnDetailResponse
            {
                Id = c.Id,
                Name = c.Name,
                Position = c.Position,
                Tasks = c.Tasks.Select(t => new TaskDetailResponse
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    DueDate = t.DueDate,
                    Priority = t.Priority,
                    Position = t.Position,
                    LabelIds = t.LabelIds
                })
            }),
            Labels = board.Labels.Select(l => new LabelDetailResponse
            {
                Id = l.Id,
                Name = l.Name,
                Color = l.Color
            })
        };

        return Ok(response);
    }
}
