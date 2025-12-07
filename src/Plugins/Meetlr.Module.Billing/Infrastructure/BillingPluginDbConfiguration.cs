using System.Reflection;
using Meetlr.Application.Common.Interfaces;

namespace Meetlr.Module.Billing.Infrastructure;

/// <summary>
/// Registers the Billing plugin's entity configurations with the main DbContext
/// </summary>
public class BillingPluginDbConfiguration : IPluginDbConfiguration
{
    /// <summary>
    /// Returns this assembly so that EF Core can find IEntityTypeConfiguration implementations
    /// </summary>
    public Assembly ConfigurationAssembly => typeof(BillingPluginDbConfiguration).Assembly;
}
