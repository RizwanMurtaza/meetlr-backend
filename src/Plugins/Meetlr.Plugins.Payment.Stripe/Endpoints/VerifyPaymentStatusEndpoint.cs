using MediatR;
using Meetlr.Plugins.Payment.Stripe.Features.Commands.VerifyPaymentStatus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Plugins.Payment.Stripe.Endpoints;

[ApiController]
[Route("api/public/payments/stripe")]
[AllowAnonymous]
public class VerifyPaymentStatusEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public VerifyPaymentStatusEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Verify payment status by querying Stripe directly
    /// </summary>
    [HttpPost("verify/booking/{bookingId}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle(Guid bookingId, CancellationToken cancellationToken)
    {
        var command = new VerifyPaymentStatusCommand
        {
            BookingId = bookingId
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result)
            return NotFound(new { message = "Booking not found or verification failed" });

        return Ok(new { message = "Payment status verified and updated" });
    }
}
