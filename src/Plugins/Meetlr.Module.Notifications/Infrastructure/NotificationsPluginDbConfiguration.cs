using System.Reflection;
using Meetlr.Application.Common.Interfaces;

namespace Meetlr.Module.Notifications.Infrastructure;

/// <summary>
/// Registers the Notifications module's entity configurations with the main DbContext
/// </summary>
public class NotificationsPluginDbConfiguration : IPluginDbConfiguration
{
    /// <summary>
    /// Returns this assembly so that EF Core can find IEntityTypeConfiguration implementations
    /// </summary>
    public Assembly ConfigurationAssembly => typeof(NotificationsPluginDbConfiguration).Assembly;
}
