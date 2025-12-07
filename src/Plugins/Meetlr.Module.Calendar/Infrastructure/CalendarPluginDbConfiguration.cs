using System.Reflection;
using Meetlr.Application.Common.Interfaces;

namespace Meetlr.Module.Calendar.Infrastructure;

/// <summary>
/// Registers the Calendar plugin's entity configurations with the main DbContext
/// </summary>
public class CalendarPluginDbConfiguration : IPluginDbConfiguration
{
    /// <summary>
    /// Returns this assembly so that EF Core can find IEntityTypeConfiguration implementations
    /// </summary>
    public Assembly ConfigurationAssembly => typeof(CalendarPluginDbConfiguration).Assembly;
}
