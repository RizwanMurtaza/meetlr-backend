namespace Meetlr.Application.Plugins.Payments;

/// <summary>
/// Factory for getting payment provider plugin instances
/// Manages all registered payment providers
/// </summary>
public interface IPaymentProviderFactory
{
    /// <summary>
    /// Get a payment provider by its ID
    /// </summary>
    IPaymentPlugin? GetProvider(string providerId);

    /// <summary>
    /// Get all registered payment providers
    /// </summary>
    IEnumerable<IPaymentPlugin> GetAllProviders();

    /// <summary>
    /// Get all enabled payment providers
    /// </summary>
    IEnumerable<IPaymentPlugin> GetEnabledProviders();

    /// <summary>
    /// Check if a provider is registered
    /// </summary>
    bool IsProviderRegistered(string providerId);
}
