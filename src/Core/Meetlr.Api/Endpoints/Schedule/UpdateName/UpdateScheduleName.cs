using MediatR;
using Meetlr.Api.Endpoints.Common;
using Meetlr.Application.Features.Schedule.Commands.UpdateScheduleName;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Schedule.UpdateName;

[Route("api/schedule")]
public class UpdateScheduleName : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public UpdateScheduleName(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPatch("{scheduleId}/name")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UpdateScheduleNameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle(Guid scheduleId, [FromBody] UpdateScheduleNameRequest request)
    {
        var command = new UpdateScheduleNameCommand
        {
            ScheduleId = scheduleId,
            UserId = CurrentUserId,
            Name = request.Name
        };

        var result = await _mediator.Send(command);

        return Ok(new UpdateScheduleNameResponse
        {
            Id = result.Id,
            Name = result.Name,
            Success = result.Success
        });
    }
}
