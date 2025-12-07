using MediatR;
using Meetlr.Module.Board.Application.Commands.UpdateLabel;
using Meetlr.Module.Board.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Board.Endpoints.Labels.UpdateLabel;

[Route("api/boards/{boardId:guid}/labels")]
public class UpdateLabelEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public UpdateLabelEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Update a label
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UpdateLabelResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle(Guid boardId, Guid id, [FromBody] UpdateLabelRequest request)
    {
        var command = new UpdateLabelCommand
        {
            BoardId = boardId,
            Id = id,
            Name = request.Name,
            Color = request.Color
        };

        var commandResponse = await _mediator.Send(command);

        var response = new UpdateLabelResponse
        {
            Id = commandResponse.Id,
            Name = commandResponse.Name,
            Color = commandResponse.Color
        };

        return Ok(response);
    }
}
