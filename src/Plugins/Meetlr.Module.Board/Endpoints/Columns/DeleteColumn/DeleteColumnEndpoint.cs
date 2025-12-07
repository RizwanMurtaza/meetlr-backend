using MediatR;
using Meetlr.Module.Board.Application.Commands.DeleteColumn;
using Meetlr.Module.Board.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Board.Endpoints.Columns.DeleteColumn;

[Route("api/boards/{boardId:guid}/columns")]
public class DeleteColumnEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public DeleteColumnEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Delete a column
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle(Guid boardId, Guid id)
    {
        var command = new DeleteColumnCommand
        {
            BoardId = boardId,
            Id = id
        };
        await _mediator.Send(command);
        return NoContent();
    }
}
