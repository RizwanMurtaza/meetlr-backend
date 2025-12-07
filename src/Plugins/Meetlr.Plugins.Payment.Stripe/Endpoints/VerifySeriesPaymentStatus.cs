using MediatR;
using Meetlr.Plugins.Payment.Stripe.Features.Commands.VerifySeriesPaymentStatus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Plugins.Payment.Stripe.Endpoints;

[ApiController]
[Route("api/public/payments")]
[AllowAnonymous]
public class VerifySeriesPaymentStatus : ControllerBase
{
    private readonly IMediator _mediator;

    public VerifySeriesPaymentStatus(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Verify and update booking series payment status from Stripe
    /// </summary>
    [HttpPost("stripe/verify/series/{seriesId}")]
    [ProducesResponseType(typeof(VerifySeriesPaymentStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle([FromRoute] Guid seriesId)
    {
        var command = new VerifySeriesPaymentStatusCommand { SeriesId = seriesId };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
