using System.Reflection;
using Meetlr.Application.Common.Interfaces;

namespace Meetlr.Module.Analytics.Infrastructure;

/// <summary>
/// Registers the Analytics plugin's entity configurations with the main DbContext
/// </summary>
public class AnalyticsPluginDbConfiguration : IPluginDbConfiguration
{
    /// <summary>
    /// Returns this assembly so that EF Core can find IEntityTypeConfiguration implementations
    /// </summary>
    public Assembly ConfigurationAssembly => typeof(AnalyticsPluginDbConfiguration).Assembly;
}
