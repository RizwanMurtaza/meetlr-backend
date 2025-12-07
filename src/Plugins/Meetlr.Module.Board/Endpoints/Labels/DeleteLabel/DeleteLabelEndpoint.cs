using MediatR;
using Meetlr.Module.Board.Application.Commands.DeleteLabel;
using Meetlr.Module.Board.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Board.Endpoints.Labels.DeleteLabel;

[Route("api/boards/{boardId:guid}/labels")]
public class DeleteLabelEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public DeleteLabelEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Delete a label
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle(Guid boardId, Guid id)
    {
        var command = new DeleteLabelCommand
        {
            BoardId = boardId,
            Id = id
        };
        await _mediator.Send(command);
        return NoContent();
    }
}
