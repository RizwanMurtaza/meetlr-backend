using MediatR;
using Meetlr.Module.Board.Application.Commands.CreateLabel;
using Meetlr.Module.Board.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Board.Endpoints.Labels.CreateLabel;

[Route("api/boards/{boardId:guid}/labels")]
public class CreateLabelEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public CreateLabelEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new label for a board
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateLabelResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle(Guid boardId, [FromBody] CreateLabelRequest request)
    {
        var command = new CreateLabelCommand
        {
            BoardId = boardId,
            Name = request.Name,
            Color = request.Color
        };

        var commandResponse = await _mediator.Send(command);

        var response = new CreateLabelResponse
        {
            Id = commandResponse.Id,
            Name = commandResponse.Name,
            Color = commandResponse.Color
        };

        return Created($"/api/boards/{boardId}/labels/{response.Id}", response);
    }
}
