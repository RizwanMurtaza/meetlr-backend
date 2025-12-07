using MediatR;
using Meetlr.Api.Endpoints.Common;
using Meetlr.Application.Features.Schedule.Commands.UpdateAvailabilitySchedule;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Schedule.Update;

[Route("api/schedule")]
public class UpdateAvailabilitySchedule : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public UpdateAvailabilitySchedule(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut("{scheduleId}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UpdateAvailabilityScheduleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle(Guid scheduleId, [FromBody] UpdateAvailabilityScheduleRequest request)
    {
        var userId = CurrentUserId;

        var command = new UpdateAvailabilityScheduleCommand
        {
            Id = scheduleId,
            UserId = userId,
            Name = request.Name,
            TimeZone = request.TimeZone,
            IsDefault = request.IsDefault,
            ScheduleType = request.ScheduleType,
            MaxBookingsPerSlot = request.MaxBookingsPerSlot,
            WeeklyHours = request.WeeklyHours.Select(w => new WeeklyHourDto
            {
                DayOfWeek = w.DayOfWeek,
                StartTime = w.StartTime,
                EndTime = w.EndTime,
                IsAvailable = w.IsAvailable
            }).ToList(),
            // Advanced Settings
            MaxBookingDaysInFuture = request.MaxBookingDaysInFuture,
            MinBookingNoticeMinutes = request.MinBookingNoticeMinutes,
            SlotIntervalMinutes = request.SlotIntervalMinutes,
            AutoDetectInviteeTimezone = request.AutoDetectInviteeTimezone
        };

        var commandResponse = await _mediator.Send(command);

        var response = new UpdateAvailabilityScheduleResponse
        {
            Id = commandResponse.Id,
            Name = commandResponse.Name,
            TimeZone = commandResponse.TimeZone,
            IsDefault = commandResponse.IsDefault,
            Success = commandResponse.Success
        };

        return Ok(response);
    }
}
