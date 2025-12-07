using MediatR;
using Meetlr.Api.Endpoints.Common;
using Meetlr.Application.Features.Schedule.Commands.UpdateScheduleTimeZone;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Schedule.UpdateTimeZone;

[Route("api/schedule")]
public class UpdateScheduleTimeZone : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public UpdateScheduleTimeZone(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Update schedule timezone
    /// </summary>
    [HttpPatch("{scheduleId}/timezone")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UpdateScheduleTimeZoneResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle(Guid scheduleId, [FromBody] UpdateScheduleTimeZoneRequest request)
    {
        try
        {
            var command = new UpdateScheduleTimeZoneCommand
            {
                ScheduleId = scheduleId,
                UserId = CurrentUserId,
                TimeZone = request.TimeZone
            };

            var result = await _mediator.Send(command);

            if (!result.Success)
                return NotFound(new { message = "Schedule not found" });

            return Ok(new UpdateScheduleTimeZoneResponse
            {
                Id = result.Id,
                TimeZone = result.TimeZone,
                Success = result.Success
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
