using MediatR;
using Meetlr.Plugins.Payment.Stripe.Features.Queries.GetBookingPaymentStatus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Plugins.Payment.Stripe.Endpoints;

[ApiController]
[Route("api/payments")]
[AllowAnonymous]
public class GetBookingPaymentStatus : ControllerBase
{
    private readonly IMediator _mediator;

    public GetBookingPaymentStatus(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get booking payment status (public endpoint for confirmation page)
    /// </summary>
    [HttpGet("{bookingId}/status")]
    [ProducesResponseType(typeof(BookingPaymentStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle([FromRoute] Guid bookingId)
    {
        var query = new GetBookingPaymentStatusQuery { BookingId = bookingId };
        var status = await _mediator.Send(query);

        if (status == null)
            return NotFound(new { message = "Booking not found" });

        return Ok(status);
    }
}
