using MediatR;
using Meetlr.Application.Features.Bookings.Commands.CancelSeriesBooking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Bookings.CancelSeries;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class CancelSeriesBooking : ControllerBase
{
    private readonly IMediator _mediator;

    public CancelSeriesBooking(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Cancel a booking series
    /// </summary>
    [HttpPost("series/{seriesId}/cancel")]
    [ProducesResponseType(typeof(CancelSeriesBookingCommandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle(Guid seriesId, [FromBody] CancelSeriesBookingRequest request)
    {
        var command = new CancelSeriesBookingCommand
        {
            SeriesId = seriesId,
            Scope = request.Scope,
            Reason = request.Reason,
            StartFromBookingId = request.StartFromBookingId
        };

        var response = await _mediator.Send(command);
        return Ok(response);
    }
}
