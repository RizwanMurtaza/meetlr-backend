using Meetlr.Application.Plugins.Payments.Models;

namespace Meetlr.Application.Plugins.Payments;

/// <summary>
/// Interface for payment provider plugins (Stripe, PayPal, Square, etc.)
/// Extends the base IEndpointPlugin to include payment-specific functionality
/// </summary>
public interface IPaymentProviderPlugin : IEndpointPlugin
{
    /// <summary>
    /// Payment provider identifier (alias for PluginId for backward compatibility)
    /// </summary>
    string ProviderId => PluginId;
    
    /// <summary>
    /// Payment provider name (alias for PluginName for backward compatibility)
    /// </summary>
    string ProviderName => PluginName;
    
    /// <summary>
    /// Generate OAuth/Connect URL for account linking
    /// </summary>
    Task<string> GenerateConnectUrlAsync(Guid userId, string returnUrl, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Complete the OAuth/Connect flow and save account details
    /// </summary>
    Task<bool> CompleteConnectAsync(string authorizationCode, Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Disconnect/Revoke the connected account
    /// </summary>
    Task<bool> DisconnectAccountAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get connection status for a user
    /// </summary>
    Task<PaymentAccountStatus?> GetAccountStatusAsync(Guid userId, CancellationToken cancellationToken = default);
    
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
    Task<bool> RefundPaymentAsync(string paymentIntentId, decimal? amount = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validate webhook signature and process webhook event
    /// </summary>
    Task<bool> ProcessWebhookAsync(string payload, string signature, Func<WebhookEvent, Task> eventHandler, CancellationToken cancellationToken = default);
}

