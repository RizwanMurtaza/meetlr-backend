using MediatR;
using Meetlr.Module.Board.Application.Commands.UpdateBoard;
using Meetlr.Module.Board.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Board.Endpoints.Boards.UpdateBoard;

[Route("api/boards")]
public class UpdateBoardEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public UpdateBoardEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Update an existing board
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UpdateBoardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle(Guid id, [FromBody] UpdateBoardRequest request)
    {
        var command = new UpdateBoardCommand
        {
            Id = id,
            Name = request.Name,
            Description = request.Description,
            Color = request.Color
        };

        var commandResponse = await _mediator.Send(command);

        var response = new UpdateBoardResponse
        {
            Id = commandResponse.Id,
            Name = commandResponse.Name,
            Description = commandResponse.Description,
            Color = commandResponse.Color,
            Position = commandResponse.Position,
            CreatedAt = commandResponse.CreatedAt,
            ModifiedAt = commandResponse.ModifiedAt
        };

        return Ok(response);
    }
}
