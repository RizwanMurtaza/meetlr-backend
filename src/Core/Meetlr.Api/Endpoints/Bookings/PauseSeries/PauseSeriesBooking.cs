using MediatR;
using Meetlr.Application.Features.Bookings.Commands.PauseSeriesBooking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Bookings.PauseSeries;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class PauseSeriesBooking : ControllerBase
{
    private readonly IMediator _mediator;

    public PauseSeriesBooking(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Pause or resume a booking series
    /// </summary>
    [HttpPost("series/{seriesId}/pause")]
    [ProducesResponseType(typeof(PauseSeriesBookingCommandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle(Guid seriesId, [FromBody] PauseSeriesBookingRequest request)
    {
        var command = new PauseSeriesBookingCommand
        {
            SeriesId = seriesId,
            Resume = request.Resume
        };

        var response = await _mediator.Send(command);
        return Ok(response);
    }
}
