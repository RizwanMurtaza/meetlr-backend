using MediatR;
using Meetlr.Api.Endpoints.Common;
using Meetlr.Application.Features.MeetlrEvents.Commands.DeleteSingleUseLink;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.SingleUseLinks.Delete;

[Route("api/single-use-links")]
public class DeleteSingleUseLink : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public DeleteSingleUseLink(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Delete a single-use booking link
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(DeleteSingleUseLinkResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle([FromRoute] Guid id)
    {
        var command = new DeleteSingleUseLinkCommand { Id = id };
        var commandResponse = await _mediator.Send(command);

        return Ok(new DeleteSingleUseLinkResponse { Success = commandResponse.Success });
    }
}
