using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Public.CreateSeriesBooking;

[ApiController]
[Route("api/public/tenant/{tenantId}/meetlrEvents")]
[AllowAnonymous]
public class CreateEventRecurringBooking : ControllerBase
{
    private readonly IMediator _mediator;

    public CreateEventRecurringBooking(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a recurring booking series
    /// </summary>
    [HttpPost("{meetlrEventId}/series")]
    [ProducesResponseType(typeof(CreateEventRecurringBookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(CreateEventRecurringBookingResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Handle([FromRoute] Guid tenantId, [FromRoute] Guid meetlrEventId, [FromBody] CreateEventRecurringBookingRequest request)
    {
        request.MeetlrEventId = meetlrEventId;
        var command = CreateEventRecurringBookingRequest.ToCommand(request);
        var commandResponse = await _mediator.Send(command);
        var response = CreateEventRecurringBookingResponse.FromCommandResponse(commandResponse);

        if (response.HasConflicts)
        {
            return Conflict(response);
        }

        return CreatedAtAction(nameof(Handle), new { tenantId, meetlrEventId, id = response.SeriesId }, response);
    }
}
