using MediatR;
using Meetlr.Plugins.Payment.Stripe.Features.Commands.DisconnectStripeAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Plugins.Payment.Stripe.Endpoints;

[ApiController]
[Route("api/payments/stripe")]
[Authorize]
public class DisconnectStripe : ControllerBase
{
    private readonly IMediator _mediator;

    public DisconnectStripe(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Disconnect Stripe account
    /// </summary>
    [HttpDelete("disconnect")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Handle()
    {
        var command = new DisconnectStripeAccountCommand();
        var success = await _mediator.Send(command);

        if (!success)
            return BadRequest(new { message = "Failed to disconnect Stripe account" });

        return Ok(new { message = "Stripe account disconnected successfully" });
    }
}
