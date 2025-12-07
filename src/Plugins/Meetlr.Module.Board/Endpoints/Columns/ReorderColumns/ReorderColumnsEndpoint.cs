using MediatR;
using Meetlr.Module.Board.Application.Commands.ReorderColumns;
using Meetlr.Module.Board.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Board.Endpoints.Columns.ReorderColumns;

[Route("api/boards/{boardId:guid}/columns")]
public class ReorderColumnsEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public ReorderColumnsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Reorder columns in a board
    /// </summary>
    [HttpPut("reorder")]
    [ProducesResponseType(typeof(ReorderColumnsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle(Guid boardId, [FromBody] ReorderColumnsRequest request)
    {
        var command = new ReorderColumnsCommand
        {
            BoardId = boardId,
            ColumnIds = request.ColumnIds
        };

        await _mediator.Send(command);
        return Ok(new ReorderColumnsResponse { Success = true });
    }
}
