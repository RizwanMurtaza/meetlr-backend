using MediatR;
using Meetlr.Application.Features.Bookings.Commands.UpdateSeriesBooking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Bookings.UpdateSeries;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class UpdateSeriesBooking : ControllerBase
{
    private readonly IMediator _mediator;

    public UpdateSeriesBooking(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Update a booking series
    /// </summary>
    [HttpPut("series/{seriesId}")]
    [ProducesResponseType(typeof(UpdateSeriesBookingCommandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle(Guid seriesId, [FromBody] UpdateSeriesBookingRequest request)
    {
        var command = new UpdateSeriesBookingCommand
        {
            SeriesId = seriesId,
            Scope = request.Scope,
            NewStartTime = request.NewStartTime,
            NewDuration = request.NewDuration,
            StartFromBookingId = request.StartFromBookingId
        };

        var response = await _mediator.Send(command);
        return Ok(response);
    }
}
