using Meetlr.Application.Plugins.Payments.Models;

namespace Meetlr.Application.Plugins.Payments;

/// <summary>
/// Interface for payment provider plugins (Stripe, PayPal, Square, etc.)
/// Extends IPlugin with payment-specific operations.
/// Connection methods (GenerateConnectUrl, CompleteConnect, Disconnect) are in base IPlugin.
/// </summary>
public interface IPaymentPlugin : IPlugin
{
    /// <summary>
    /// Get the connected account ID for a user (provider-specific account identifier)
    /// Returns null if user is not connected to this payment provider
    /// </summary>
    Task<string?> GetConnectedAccountIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a payment intent/order for a booking
    /// </summary>
    Task<PaymentIntentResult> CreatePaymentIntentAsync(
        Guid bookingId,
        decimal amount,
        string currency,
        string connectedAccountId,
        string description,
        Dictionary<string, string> metadata,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Process refund for a payment
    /// </summary>
    Task<bool> RefundPaymentAsync(
        string paymentIntentId,
        decimal? amount = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate webhook signature and process webhook event
    /// </summary>
    Task<bool> ProcessWebhookAsync(
        string payload,
        string signature,
        Func<WebhookEvent, Task> eventHandler,
        CancellationToken cancellationToken = default);
}
