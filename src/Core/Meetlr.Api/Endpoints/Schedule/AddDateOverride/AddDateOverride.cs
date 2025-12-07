using MediatR;
using Meetlr.Api.Endpoints.Common;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Schedule.AddDateOverride;

[Route("api/schedule")]
public class AddDateOverride : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public AddDateOverride(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("{scheduleId}/date-override")]
    public async Task<IActionResult> Handle(Guid scheduleId, [FromBody] AddDateOverrideRequest request)
    {
        var userId = CurrentUserId;

        var command = AddDateOverrideRequest.ToCommand(request, scheduleId, userId);
        var commandResponse = await _mediator.Send(command);
        var response = AddDateOverrideResponse.FromCommandResponse(commandResponse);

        return Ok(response);
    }
}
