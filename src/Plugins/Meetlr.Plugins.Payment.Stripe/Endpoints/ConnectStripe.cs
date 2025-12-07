using MediatR;
using Meetlr.Plugins.Payment.Stripe.Endpoints.Models;
using Meetlr.Plugins.Payment.Stripe.Features.Commands.ConnectStripeAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Plugins.Payment.Stripe.Endpoints;

[ApiController]
[Route("api/payments/stripe")]
[Authorize]
public class ConnectStripe : ControllerBase
{
    private readonly IMediator _mediator;

    public ConnectStripe(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Generate Stripe Connect OAuth URL
    /// </summary>
    [HttpPost("connect")]
    [ProducesResponseType(typeof(ConnectStripeResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ConnectStripeResponse>> Handle([FromBody] ConnectStripeRequest request)
    {
        var command = new ConnectStripeAccountCommand
        {
            ReturnUrl = request.ReturnUrl
        };

        var connectUrl = await _mediator.Send(command);

        return Ok(new ConnectStripeResponse(connectUrl));
    }
}
