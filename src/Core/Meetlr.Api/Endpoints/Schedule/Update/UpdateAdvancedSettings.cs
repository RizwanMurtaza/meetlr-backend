using MediatR;
using Meetlr.Api.Endpoints.Common;
using Meetlr.Application.Features.Schedule.Commands.UpdateAdvancedSettings;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Schedule.Update;

[Route("api/schedule")]
public class UpdateAdvancedSettings : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public UpdateAdvancedSettings(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPatch("{scheduleId}/advanced-settings")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UpdateAdvancedSettingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle(Guid scheduleId, [FromBody] UpdateAdvancedSettingsRequest request)
    {
        var userId = CurrentUserId;

        var command = new UpdateAdvancedSettingsCommand
        {
            ScheduleId = scheduleId,
            UserId = userId,
            MaxBookingDaysInFuture = request.MaxBookingDaysInFuture,
            MinBookingNoticeMinutes = request.MinBookingNoticeMinutes,
            SlotIntervalMinutes = request.SlotIntervalMinutes,
            AutoDetectInviteeTimezone = request.AutoDetectInviteeTimezone
        };

        var commandResponse = await _mediator.Send(command);

        var response = new UpdateAdvancedSettingsResponse
        {
            Success = commandResponse.Success,
            MaxBookingDaysInFuture = commandResponse.MaxBookingDaysInFuture,
            MinBookingNoticeMinutes = commandResponse.MinBookingNoticeMinutes,
            SlotIntervalMinutes = commandResponse.SlotIntervalMinutes,
            AutoDetectInviteeTimezone = commandResponse.AutoDetectInviteeTimezone
        };

        return Ok(response);
    }
}
