using MediatR;
using Meetlr.Module.Board.Application.Commands.CreateColumn;
using Meetlr.Module.Board.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Board.Endpoints.Columns.CreateColumn;

[Route("api/boards/{boardId:guid}/columns")]
public class CreateColumnEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public CreateColumnEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new column in a board
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateColumnResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle(Guid boardId, [FromBody] CreateColumnRequest request)
    {
        var command = new CreateColumnCommand
        {
            BoardId = boardId,
            Name = request.Name
        };

        var commandResponse = await _mediator.Send(command);

        var response = new CreateColumnResponse
        {
            Id = commandResponse.Id,
            Name = commandResponse.Name,
            Position = commandResponse.Position
        };

        return Created($"/api/boards/{boardId}/columns/{response.Id}", response);
    }
}
