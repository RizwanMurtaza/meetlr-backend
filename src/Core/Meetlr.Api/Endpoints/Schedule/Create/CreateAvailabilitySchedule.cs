using MediatR;
using Meetlr.Api.Endpoints.Common;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Schedule.Create;

[Route("api/schedule")]
public class CreateAvailabilitySchedule : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public CreateAvailabilitySchedule(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Handle([FromBody] CreateScheduleRequest request)
    {
        var userId = CurrentUserId;

        var command = CreateScheduleRequest.ToCommand(request, userId);
        var commandResponse = await _mediator.Send(command);
        var response = CreateAvailabilityScheduleResponse.FromCommandResponse(commandResponse);

        return CreatedAtAction(nameof(Handle), new { id = response.Id }, response);
    }
}
