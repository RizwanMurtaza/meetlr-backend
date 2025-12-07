using Meetlr.Application.Plugins.MeetingTypes;
using Meetlr.Application.Plugins.Payments;
using Meetlr.Domain.Enums;

namespace Meetlr.Application.Plugins;

/// <summary>
/// Unified factory for retrieving plugins of any category
/// </summary>
public interface IPluginFactory
{
    /// <summary>
    /// Get a plugin by its unique identifier
    /// </summary>
    IPlugin? GetPlugin(string pluginId);

    /// <summary>
    /// Get all registered plugins
    /// </summary>
    IEnumerable<IPlugin> GetAllPlugins();

    /// <summary>
    /// Get all plugins in a specific category
    /// </summary>
    IEnumerable<IPlugin> GetPluginsByCategory(PluginCategory category);

    /// <summary>
    /// Get all enabled plugins
    /// </summary>
    IEnumerable<IPlugin> GetEnabledPlugins();

    /// <summary>
    /// Get all payment plugins
    /// </summary>
    IEnumerable<IPaymentPlugin> GetPaymentPlugins();

    /// <summary>
    /// Get a specific payment plugin by ID
    /// </summary>
    IPaymentPlugin? GetPaymentPlugin(string pluginId);

    /// <summary>
    /// Get all meeting types plugins
    /// </summary>
    IEnumerable<IMeetingTypesPlugin> GetMeetingTypinsPlugins();

    /// <summary>
    /// Get a specific meeting types plugin by ID
    /// </summary>
    IMeetingTypesPlugin? GetMeetingTypesPlugin(string pluginId);

    /// <summary>
    /// Get meeting types plugin by location type
    /// </summary>
    IMeetingTypesPlugin? GetMeetingTypesPluginByLocationType(MeetingLocationType locationType);
}
