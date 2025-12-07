using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Bookings.Reschedule;

[ApiController]
[Route("api/bookings")]
[AllowAnonymous]
public class RescheduleBooking : ControllerBase
{
    private readonly IMediator _mediator;

    public RescheduleBooking(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Step 1: Verify identity before allowing reschedule.
    /// Validates confirmation token and email/phone match.
    /// Returns booking details and whether reschedule is allowed.
    /// </summary>
    [HttpPost("{id}/reschedule/verify")]
    [ProducesResponseType(typeof(VerifyIdentityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyIdentity(Guid id, [FromBody] VerifyIdentityRequest request)
    {
        var query = VerifyIdentityRequest.ToQuery(request, id);
        var queryResponse = await _mediator.Send(query);
        var response = VerifyIdentityResponse.FromQueryResponse(queryResponse);

        return Ok(response);
    }

    /// <summary>
    /// Step 2: Reschedule the booking to a new time slot.
    /// Re-verifies identity and validates the new time slot.
    /// </summary>
    [HttpPost("{id}/reschedule")]
    [ProducesResponseType(typeof(RescheduleBookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle(Guid id, [FromBody] RescheduleBookingRequest request)
    {
        var command = RescheduleBookingRequest.ToCommand(request, id);
        var commandResponse = await _mediator.Send(command);
        var response = RescheduleBookingResponse.FromCommandResponse(commandResponse);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}
