namespace Meetlr.Application.Plugins;

/// <summary>
/// Interface for plugins that need to register their own API endpoints
/// </summary>
public interface IEndpointPlugin : IPlugin
{
    /// <summary>
    /// Returns the plugin's controller types that should be registered
    /// The hosting application will scan these types and register them
    /// </summary>
    Type[] GetControllerTypes();
}
