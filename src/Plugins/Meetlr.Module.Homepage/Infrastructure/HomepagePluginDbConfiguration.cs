using System.Reflection;
using Meetlr.Application.Common.Interfaces;

namespace Meetlr.Module.Homepage.Infrastructure;

/// <summary>
/// Registers the Homepage plugin's entity configurations with the main DbContext
/// </summary>
public class HomepagePluginDbConfiguration : IPluginDbConfiguration
{
    /// <summary>
    /// Returns this assembly so that EF Core can find IEntityTypeConfiguration implementations
    /// </summary>
    public Assembly ConfigurationAssembly => typeof(HomepagePluginDbConfiguration).Assembly;
}
