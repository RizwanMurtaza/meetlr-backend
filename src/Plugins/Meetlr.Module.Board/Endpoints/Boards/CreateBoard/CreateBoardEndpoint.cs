using MediatR;
using Meetlr.Module.Board.Application.Commands.CreateBoard;
using Meetlr.Module.Board.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Board.Endpoints.Boards.CreateBoard;

[Route("api/boards")]
public class CreateBoardEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public CreateBoardEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new board with default columns
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateBoardResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle([FromBody] CreateBoardRequest request)
    {
        var command = new CreateBoardCommand
        {
            Name = request.Name,
            Description = request.Description,
            Color = request.Color
        };

        var commandResponse = await _mediator.Send(command);

        var response = new CreateBoardResponse
        {
            Id = commandResponse.Id,
            Name = commandResponse.Name,
            Description = commandResponse.Description,
            Color = commandResponse.Color,
            Position = commandResponse.Position,
            CreatedAt = commandResponse.CreatedAt
        };

        return CreatedAtAction(
            nameof(GetBoardById.GetBoardByIdEndpoint.Handle),
            "GetBoardByIdEndpoint",
            new { id = response.Id },
            response);
    }
}
