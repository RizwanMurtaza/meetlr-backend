namespace Meetlr.Application.Features.Plugins.Queries.GetInstalledPlugins;

public class GetInstalledPluginsResponse
{
    public List<InstalledPluginInfo> Plugins { get; set; } = new();
}