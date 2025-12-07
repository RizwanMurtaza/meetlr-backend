using MediatR;
using Meetlr.Api.Endpoints.Common;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.SingleUseLinks.Create;

[Route("api/single-use-links")]
public class CreateSingleUseLink : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public CreateSingleUseLink(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new single-use booking link for an event type
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateSingleUseLinkResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle([FromBody] CreateSingleUseLinkRequest request)
    {
        var command = CreateSingleUseLinkRequest.ToCommand(request);
        var commandResponse = await _mediator.Send(command);
        var response = CreateSingleUseLinkResponse.FromCommandResponse(commandResponse);

        return CreatedAtAction(nameof(Handle), new { id = response.Id }, response);
    }
}
