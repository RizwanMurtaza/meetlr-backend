using MediatR;
using Meetlr.Plugins.Payment.Stripe.Features.Commands.ProcessStripeWebhook;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

namespace Meetlr.Plugins.Payment.Stripe.Endpoints;

[ApiController]
[Route("api/payments/stripe")]
[AllowAnonymous]
public class StripeWebhook : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeWebhook> _logger;

    public StripeWebhook(
        IMediator mediator,
        IConfiguration configuration,
        ILogger<StripeWebhook> logger)
    {
        _mediator = mediator;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Stripe webhook endpoint
    /// </summary>
    [HttpPost("webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Handle()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var stripeSignature = Request.Headers["Stripe-Signature"].ToString();
        var webhookSecret = _configuration["Stripe:WebhookSecret"];

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                stripeSignature,
                webhookSecret
            );

            _logger.LogInformation("Received Stripe webhook: {EventType}", stripeEvent.Type);

            // Handle account update events (for Stripe Connect)
            if (stripeEvent.Type == "account.updated")
            {
                var account = stripeEvent.Data.Object as Account;
                if (account != null)
                {
                    var syncCommand = new Features.Commands.SyncStripeAccount.SyncStripeAccountCommand
                    {
                        StripeAccountId = account.Id
                    };

                    await _mediator.Send(syncCommand);
                }
            }
            // Handle payment intent events
            else if (stripeEvent.Type.StartsWith("payment_intent.") || stripeEvent.Type.StartsWith("charge."))
            {
                PaymentIntent? paymentIntent = null;
                Charge? charge = null;

                if (stripeEvent.Type.StartsWith("payment_intent."))
                {
                    paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                }
                else if (stripeEvent.Type == "charge.refunded")
                {
                    charge = stripeEvent.Data.Object as Charge;
                    // The PaymentIntentId property contains the ID as a string
                    if (!string.IsNullOrEmpty(charge?.PaymentIntentId))
                    {
                        // For charge.refunded, we create a minimal PaymentIntent with just the ID
                        paymentIntent = new PaymentIntent { Id = charge.PaymentIntentId };
                    }
                }

                if (paymentIntent != null)
                {
                    // Get status string from either PaymentIntent or Charge
                    string status = "unknown";
                    if (!string.IsNullOrEmpty(paymentIntent.Status))
                    {
                        status = paymentIntent.Status;
                    }
                    else if (!string.IsNullOrEmpty(charge?.Status))
                    {
                        status = charge.Status;
                    }

                    var command = new ProcessStripeWebhookCommand
                    {
                        EventType = stripeEvent.Type,
                        PaymentIntentId = paymentIntent.Id,
                        Status = status,
                        Metadata = paymentIntent.Metadata ?? charge?.Metadata ?? new Dictionary<string, string>()
                    };

                    await _mediator.Send(command);
                }
            }

            return Ok();
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook signature verification failed");
            return BadRequest(new { error = "Invalid signature" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook");
            return StatusCode(500, new { error = "Webhook processing failed" });
        }
    }
}
