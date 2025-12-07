using MediatR;
using Meetlr.Plugins.Payment.Stripe.Features.Commands.SyncStripeAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Plugins.Payment.Stripe.Endpoints;

[ApiController]
[Route("api/payments/stripe")]
[Authorize]
public class SyncStripeAccount : ControllerBase
{
    private readonly IMediator _mediator;

    public SyncStripeAccount(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Manually sync Stripe Connect account status from Stripe API
    /// </summary>
    [HttpPost("sync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Handle()
    {
        // Get the user's Stripe account ID from the database
        // This will be retrieved via the command handler using CurrentUserService
        var command = new SyncStripeAccountCommand
        {
            StripeAccountId = string.Empty // Will be looked up by user ID in handler
        };

        var success = await _mediator.Send(command);

        if (!success)
            return NotFound(new { message = "Stripe account not found or sync failed" });

        return Ok(new { message = "Stripe account synced successfully" });
    }
}
