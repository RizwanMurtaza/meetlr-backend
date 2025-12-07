using Meetlr.Application.Plugins.Payments;

namespace Meetlr.Infrastructure.Plugins.Payments;

/// <summary>
/// Factory implementation for payment provider plugins
/// </summary>
public class PaymentProviderFactory : IPaymentProviderFactory
{
    private readonly IEnumerable<IPaymentPlugin> _providers;

    public PaymentProviderFactory(IEnumerable<IPaymentPlugin> providers)
    {
        _providers = providers;
    }

    public IPaymentPlugin? GetProvider(string providerId)
    {
        return _providers.FirstOrDefault(p =>
            p.PluginId.Equals(providerId, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<IPaymentPlugin> GetAllProviders()
    {
        return _providers;
    }

    public IEnumerable<IPaymentPlugin> GetEnabledProviders()
    {
        return _providers.Where(p => p.IsEnabled);
    }

    public bool IsProviderRegistered(string providerId)
    {
        return _providers.Any(p =>
            p.PluginId.Equals(providerId, StringComparison.OrdinalIgnoreCase));
    }
}
