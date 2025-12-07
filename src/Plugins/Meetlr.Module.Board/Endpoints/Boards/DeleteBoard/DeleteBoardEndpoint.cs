using MediatR;
using Meetlr.Module.Board.Application.Commands.DeleteBoard;
using Meetlr.Module.Board.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Board.Endpoints.Boards.DeleteBoard;

[Route("api/boards")]
public class DeleteBoardEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public DeleteBoardEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Delete a board
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle(Guid id)
    {
        var command = new DeleteBoardCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
