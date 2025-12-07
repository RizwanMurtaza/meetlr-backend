using System.Reflection;
using Meetlr.Application.Common.Interfaces;

namespace Meetlr.Module.SlotInvitation.Infrastructure;

/// <summary>
/// Registers the SlotInvitation plugin's entity configurations with the main DbContext
/// </summary>
public class SlotInvitationPluginDbConfiguration : IPluginDbConfiguration
{
    /// <summary>
    /// Returns this assembly so that EF Core can find IEntityTypeConfiguration implementations
    /// </summary>
    public Assembly ConfigurationAssembly => typeof(SlotInvitationPluginDbConfiguration).Assembly;
}
