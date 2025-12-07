using System.Reflection;
using Meetlr.Application.Common.Interfaces;

namespace Meetlr.Module.Board.Infrastructure;

/// <summary>
/// Registers the Board plugin's entity configurations with the main DbContext
/// </summary>
public class BoardPluginDbConfiguration : IPluginDbConfiguration
{
    /// <summary>
    /// Returns this assembly so that EF Core can find IEntityTypeConfiguration implementations
    /// </summary>
    public Assembly ConfigurationAssembly => typeof(BoardPluginDbConfiguration).Assembly;
}
