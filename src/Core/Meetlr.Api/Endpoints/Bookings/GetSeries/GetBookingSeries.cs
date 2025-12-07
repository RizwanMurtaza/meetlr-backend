using MediatR;
using Meetlr.Application.Features.Bookings.Queries.GetBookingSeries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Bookings.GetSeries;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class GetBookingSeries : ControllerBase
{
    private readonly IMediator _mediator;

    public GetBookingSeries(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get details of a booking series
    /// </summary>
    [HttpGet("series/{seriesId}")]
    [ProducesResponseType(typeof(GetBookingSeriesQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle(Guid seriesId)
    {
        var query = new GetBookingSeriesQuery { SeriesId = seriesId };
        var response = await _mediator.Send(query);
        return Ok(response);
    }
}
