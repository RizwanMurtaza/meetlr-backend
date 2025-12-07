using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Bookings.CreateRecurring;

[ApiController]
[Route("api/bookings")]
[AllowAnonymous]
public class CreateRecurringBooking : ControllerBase
{
    private readonly IMediator _mediator;

    public CreateRecurringBooking(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a recurring booking series
    /// </summary>
    [HttpPost("recurring")]
    [ProducesResponseType(typeof(CreateRecurringBookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(CreateRecurringBookingResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Handle([FromBody] CreateRecurringBookingRequest request)
    {
        var command = CreateRecurringBookingRequest.ToCommand(request);
        var commandResponse = await _mediator.Send(command);
        var response = CreateRecurringBookingResponse.FromCommandResponse(commandResponse);

        if (response.HasConflicts)
        {
            return Conflict(response);
        }

        return CreatedAtAction(nameof(Handle), new { id = response.SeriesId }, response);
    }
}
