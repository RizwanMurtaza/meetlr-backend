using MediatR;
using Meetlr.Api.Endpoints.Common;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Profile.Update;

[Route("api/profile")]
public class UpdateProfile : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public UpdateProfile(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UpdateProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle([FromBody] UpdateProfileRequest request)
    {
        var userId = CurrentUserId;

        var command = UpdateProfileRequest.ToCommand(request, userId);
        var commandResponse = await _mediator.Send(command);
        var response = UpdateProfileResponse.FromCommandResponse(commandResponse);

        return Ok(response);
    }
}
