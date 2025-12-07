using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Bookings.Create;

[ApiController]
[Route("api/bookings")]
[AllowAnonymous]
public class CreateBooking : ControllerBase
{
    private readonly IMediator _mediator;

    public CreateBooking(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new booking for an event type
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateBookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Handle([FromBody] CreateBookingRequest request)
    {
        var command = CreateBookingRequest.ToCommand(request);
        var commandResponse = await _mediator.Send(command);
        var response = CreateBookingResponse.FromCommandResponse(commandResponse);

        return CreatedAtAction(nameof(Handle), new { id = response.BookingId }, response);
    }
}
