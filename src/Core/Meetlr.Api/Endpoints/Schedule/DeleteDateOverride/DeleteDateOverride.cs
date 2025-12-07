using MediatR;
using Meetlr.Api.Endpoints.Common;
using Meetlr.Application.Features.Schedule.Commands.DeleteDateOverride;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Schedule.DeleteDateOverride;

[Route("api/schedule")]
public class DeleteDateOverride : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public DeleteDateOverride(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpDelete("{scheduleId}/date-override/{overrideId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle(Guid scheduleId, Guid overrideId)
    {
        var command = new DeleteDateOverrideCommand
        {
            ScheduleId = scheduleId,
            OverrideId = overrideId,
            UserId = CurrentUserId
        };

        var result = await _mediator.Send(command);

        if (!result.Success)
            return NotFound();

        return NoContent();
    }
}
