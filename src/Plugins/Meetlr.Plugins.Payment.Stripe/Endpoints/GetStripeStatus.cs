using MediatR;
using Meetlr.Plugins.Payment.Stripe.Features.Queries.GetStripeAccountStatus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Plugins.Payment.Stripe.Endpoints;

[ApiController]
[Route("api/payments/stripe")]
[Authorize]
public class GetStripeStatus : ControllerBase
{
    private readonly IMediator _mediator;

    public GetStripeStatus(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get Stripe account status
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(StripeAccountStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle()
    {
        var query = new GetStripeAccountStatusQuery();
        var status = await _mediator.Send(query);

        if (status == null)
            return NotFound(new { message = "No Stripe account connected" });

        return Ok(status);
    }
}
