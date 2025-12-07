using MediatR;
using Meetlr.Plugins.Payment.Stripe.Features.Queries.GetSeriesPaymentStatus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Plugins.Payment.Stripe.Endpoints;

[ApiController]
[Route("api/payments")]
[AllowAnonymous]
public class GetSeriesPaymentStatus : ControllerBase
{
    private readonly IMediator _mediator;

    public GetSeriesPaymentStatus(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get booking series payment status (public endpoint for confirmation page)
    /// </summary>
    [HttpGet("series/{seriesId}/status")]
    [ProducesResponseType(typeof(SeriesPaymentStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle([FromRoute] Guid seriesId)
    {
        var query = new GetSeriesPaymentStatusQuery { SeriesId = seriesId };
        var status = await _mediator.Send(query);

        if (status == null)
            return NotFound(new { message = "Booking series not found" });

        return Ok(status);
    }
}
