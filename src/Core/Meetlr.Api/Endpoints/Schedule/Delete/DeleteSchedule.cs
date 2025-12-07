using MediatR;
using Meetlr.Api.Endpoints.Common;
using Meetlr.Application.Features.Schedule.Commands.DeleteAvailabilitySchedule;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Schedule.Delete;

[Route("api/schedule")]
public class DeleteSchedule : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public DeleteSchedule(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpDelete("{scheduleId}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(DeleteScheduleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle(Guid scheduleId)
    {
        var userId = CurrentUserId;

        var command = new DeleteAvailabilityScheduleCommand
        {
            Id = scheduleId,
            UserId = userId
        };

        var commandResponse = await _mediator.Send(command);

        var response = new DeleteScheduleResponse
        {
            Success = commandResponse.Success
        };

        return Ok(response);
    }
}
