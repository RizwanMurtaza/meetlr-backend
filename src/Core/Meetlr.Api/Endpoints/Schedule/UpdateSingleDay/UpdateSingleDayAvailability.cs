using MediatR;
using Meetlr.Api.Endpoints.Common;
using Meetlr.Application.Features.Schedule.Commands.UpdateSingleDayAvailability;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Schedule.UpdateSingleDay;

[Route("api/schedule")]
public class UpdateSingleDayAvailability : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public UpdateSingleDayAvailability(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPatch("{scheduleId}/day")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UpdateSingleDayAvailabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle(Guid scheduleId, [FromBody] UpdateSingleDayAvailabilityRequest request)
    {
        var userId = CurrentUserId;

        var command = new UpdateSingleDayCommandRequest
        {
            ScheduleId = scheduleId,
            UserId = userId,
            DayOfWeek = request.DayOfWeek,
            IsAvailable = request.IsAvailable,
            TimeSlots = request.TimeSlots.Select(s => new TimeSlotDto
            {
                StartTime = s.StartTime,
                EndTime = s.EndTime
            }).ToList()
        };

        var commandResponse = await _mediator.Send(command);

        var response = new UpdateSingleDayAvailabilityResponse
        {
            ScheduleId = commandResponse.ScheduleId,
            DayOfWeek = commandResponse.DayOfWeek,
            Success = commandResponse.Success,
            Message = commandResponse.Message
        };

        return Ok(response);
    }
}
