using MediatR;
using Meetlr.Api.Endpoints.Common;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.EventThemes.CreateOrUpdate;

[Route("api/meetlr-events/{eventId}/theme")]
public class CreateOrUpdateEventTheme : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public CreateOrUpdateEventTheme(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create or update the theme for a Meetlr event
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(CreateOrUpdateEventThemeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle([FromRoute] Guid eventId, [FromBody] CreateOrUpdateEventThemeRequest request)
    {
        var command = CreateOrUpdateEventThemeRequest.ToCommand(eventId, request);
        var commandResponse = await _mediator.Send(command);
        var response = CreateOrUpdateEventThemeResponse.FromCommandResponse(commandResponse);

        return Ok(response);
    }
}
