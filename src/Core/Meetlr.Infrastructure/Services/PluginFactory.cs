using Meetlr.Application.Plugins;
using Meetlr.Application.Plugins.MeetingTypes;
using Meetlr.Application.Plugins.Payments;
using Meetlr.Domain.Enums;

namespace Meetlr.Infrastructure.Services;

/// <summary>
/// Implementation of IPluginFactory that uses DI to collect all registered plugins
/// </summary>
public class PluginFactory : IPluginFactory
{
    private readonly IEnumerable<IPlugin> _plugins;

    public PluginFactory(IEnumerable<IPlugin> plugins)
    {
        _plugins = plugins;
    }

    public IPlugin? GetPlugin(string pluginId)
    {
        return _plugins.FirstOrDefault(p =>
            p.PluginId.Equals(pluginId, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<IPlugin> GetAllPlugins()
    {
        return _plugins;
    }

    public IEnumerable<IPlugin> GetPluginsByCategory(PluginCategory category)
    {
        return _plugins.Where(p => p.Category == category);
    }

    public IEnumerable<IPlugin> GetEnabledPlugins()
    {
        return _plugins.Where(p => p.IsEnabled);
    }

    public IEnumerable<IPaymentPlugin> GetPaymentPlugins()
    {
        return _plugins
            .Where(p => p.Category == PluginCategory.Payment)
            .OfType<IPaymentPlugin>();
    }

    public IPaymentPlugin? GetPaymentPlugin(string pluginId)
    {
        return GetPaymentPlugins()
            .FirstOrDefault(p => p.PluginId.Equals(pluginId, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<IMeetingTypesPlugin> GetMeetingTypinsPlugins()
    {
        return _plugins
            .Where(p => p.Category == PluginCategory.VideoConferencing)
            .OfType<IMeetingTypesPlugin>();
    }

    public IMeetingTypesPlugin? GetMeetingTypesPlugin(string pluginId)
    {
        return GetMeetingTypinsPlugins()
            .FirstOrDefault(p => p.PluginId.Equals(pluginId, StringComparison.OrdinalIgnoreCase));
    }

    public IMeetingTypesPlugin? GetMeetingTypesPluginByLocationType(MeetingLocationType locationType)
    {
        return GetMeetingTypinsPlugins()
            .FirstOrDefault(p => p.LocationType == locationType);
    }
}
