using MediatR;
using Meetlr.Api.Endpoints.Common;
using Meetlr.Api.Endpoints.UserSettings.Get;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.UserSettings.Update;

[Route("api/user-settings")]
public class UpdateUserSettings : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public UpdateUserSettings(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserSettingsResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle([FromBody] UpdateUserSettingsRequest request, CancellationToken cancellationToken)
    {
        var command = request.ToCommand(CurrentUserId);

        var result = await _mediator.Send(command, cancellationToken);

        var response = UserSettingsResponseModel.FromCommandResponse(result);

        return Ok(response);
    }
}
