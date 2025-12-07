using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Bookings.Cancel;

[ApiController]
[Route("api/bookings")]
[AllowAnonymous]
public class CancelBooking : ControllerBase
{
    private readonly IMediator _mediator;

    public CancelBooking(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Cancel a booking
    /// </summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(CancelBookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle(Guid id, [FromBody] CancelBookingRequest request)
    {
        var command = CancelBookingRequest.ToCommand(request, id);
        var commandResponse = await _mediator.Send(command);
        var response = CancelBookingResponse.FromCommandResponse(commandResponse);

        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }
}
