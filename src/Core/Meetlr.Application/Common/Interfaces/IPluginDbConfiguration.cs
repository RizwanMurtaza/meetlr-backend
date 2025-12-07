using System.Reflection;

namespace Meetlr.Application.Common.Interfaces;

/// <summary>
/// Interface for plugins to register their EF Core configurations
/// Plugins implementing this interface will have their entity configurations
/// automatically applied to the ApplicationDbContext
/// </summary>
public interface IPluginDbConfiguration
{
    /// <summary>
    /// The assembly containing IEntityTypeConfiguration implementations
    /// </summary>
    Assembly ConfigurationAssembly { get; }
}
