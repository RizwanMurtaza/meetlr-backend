using MediatR;
using Meetlr.Module.Calendar.Application.Commands.UpdateCalendarSettings;
using Meetlr.Module.Calendar.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Calendar.Endpoints.UpdateSettings;

[Route("api/calendar")]
public class UpdateCalendarSettings : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public UpdateCalendarSettings(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Update calendar integration settings
    /// </summary>
    /// <param name="calendarIntegrationId">Calendar integration ID</param>
    /// <param name="request">Updated settings</param>
    [HttpPatch("{calendarIntegrationId}/settings")]
    [ProducesResponseType(typeof(UpdateCalendarSettingsCommandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid calendarIntegrationId,
        [FromBody] UpdateCalendarSettingsRequest request)
    {
        var command = new UpdateCalendarSettingsCommand
        {
            ScheduleId = request.ScheduleId,
            CalendarIntegrationId = calendarIntegrationId,
            IsPrimaryCalendar = request.IsPrimaryCalendar,
            CheckForConflicts = request.CheckForConflicts,
            AddEventsToCalendar = request.AddEventsToCalendar,
            IncludeBuffers = request.IncludeBuffers,
            AutoSync = request.AutoSync
        };

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Message });
        }

        return Ok(result);
    }
}
