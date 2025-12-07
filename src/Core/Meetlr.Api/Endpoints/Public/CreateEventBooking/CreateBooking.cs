using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Public.CreateEventBooking;

[ApiController]
[Route("api/public/tenant/{tenantId}/meetlrEvents")]
[AllowAnonymous]
public class CreateEventBooking : ControllerBase
{
    private readonly IMediator _mediator;

    public CreateEventBooking(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new booking for an event type
    /// </summary>
    [HttpPost("{meetlrEventId}/booking")]
    [ProducesResponseType(typeof(CreateEventBookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Handle([FromRoute] Guid tenantId, [FromRoute] Guid meetlrEventId, [FromBody] CreateEventBookingRequest request)
    {
        request.MeetlrEventId = meetlrEventId;
        var command = CreateEventBookingRequest.ToCommand(request);
        var commandResponse = await _mediator.Send(command);
        var response = CreateEventBookingResponse.FromCommandResponse(commandResponse);

        return CreatedAtAction(nameof(Handle), new { tenantId, meetlrEventId, id = response.BookingId }, response);
    }
}
