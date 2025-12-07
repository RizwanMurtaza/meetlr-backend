using MediatR;
using Meetlr.Plugins.Payment.Stripe.Endpoints.Models;
using Meetlr.Plugins.Payment.Stripe.Features.Commands.CompleteStripeConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Plugins.Payment.Stripe.Endpoints;

[ApiController]
[Route("api/payments/stripe")]
[Authorize]
public class CompleteStripeConnect : ControllerBase
{
    private readonly IMediator _mediator;

    public CompleteStripeConnect(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Complete Stripe Connect OAuth flow
    /// </summary>
    [HttpPost("complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Handle([FromBody] CompleteStripeConnectRequest request)
    {
        var command = new CompleteStripeConnectCommand
        {
            AuthorizationCode = request.Code
        };

        var success = await _mediator.Send(command);

        if (!success)
            return BadRequest(new { message = "Failed to connect Stripe account" });

        return Ok(new { message = "Stripe account connected successfully" });
    }
}
