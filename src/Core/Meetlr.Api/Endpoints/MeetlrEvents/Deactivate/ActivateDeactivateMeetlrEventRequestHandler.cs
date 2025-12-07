using MediatR;
using Meetlr.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.MeetlrEvents.Deactivate;

[ApiController]
[Route("api/event-types")]
[Authorize]
public class ActivateDeactivateMeetlrEventRequestHandler : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public ActivateDeactivateMeetlrEventRequestHandler(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Activate or deactivate an event type.
    /// When deactivating, optionally cancel future bookings with a reason.
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(UpdateMeetlrEventStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle(Guid id, [FromBody] ActivateDeactivateMeetlrEventRequest request)
    {
        var userId = _currentUserService.UserId;
        if (userId == null || userId == Guid.Empty)
            return Unauthorized();

        // Validate: CancellationReason required when deactivating with CancelFutureBookings
        if (!request.IsActive && request.CancelFutureBookings && string.IsNullOrWhiteSpace(request.CancellationReason))
        {
            return BadRequest(new { error = "Cancellation reason is required when cancelling future bookings" });
        }

        var command = ActivateDeactivateMeetlrEventRequest.ToCommand(request, id, userId.Value);
        var commandResponse = await _mediator.Send(command);
        var response = UpdateMeetlrEventStatusResponse.FromCommandResponse(commandResponse);

        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }
}
