namespace Meetlr.Application.Features.Plugins.Queries.GetAvailablePlugins;

public class GetAvailablePluginsResponse
{
    public List<PluginInfo> Plugins { get; set; } = new();
}