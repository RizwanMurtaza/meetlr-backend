using MediatR;
using Meetlr.Api.Endpoints.Common;
using Meetlr.Application.Features.Schedule.Commands.SetDefaultSchedule;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Schedule.SetDefault;

[Route("api/schedule")]
public class SetDefaultSchedule : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public SetDefaultSchedule(IMediator mediator)
    {
        _mediator = mediator;
    }

    
    [HttpPut("{scheduleId}/set-default")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(SetDefaultScheduleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle(Guid scheduleId)
    {
        var userId = CurrentUserId;

        var command = new SetDefaultScheduleCommand
        {
            Id = scheduleId,
            UserId = userId
        };

        var commandResponse = await _mediator.Send(command);

        var response = new SetDefaultScheduleResponse
        {
            Success = commandResponse.Success
        };

        return Ok(response);
    }
}
