using MediatR;
using Meetlr.Module.Board.Application.Commands.UpdateColumn;
using Meetlr.Module.Board.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Board.Endpoints.Columns.UpdateColumn;

[Route("api/boards/{boardId:guid}/columns")]
public class UpdateColumnEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public UpdateColumnEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Update a column
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UpdateColumnResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle(Guid boardId, Guid id, [FromBody] UpdateColumnRequest request)
    {
        var command = new UpdateColumnCommand
        {
            BoardId = boardId,
            Id = id,
            Name = request.Name
        };

        var commandResponse = await _mediator.Send(command);

        var response = new UpdateColumnResponse
        {
            Id = commandResponse.Id,
            Name = commandResponse.Name,
            Position = commandResponse.Position
        };

        return Ok(response);
    }
}
