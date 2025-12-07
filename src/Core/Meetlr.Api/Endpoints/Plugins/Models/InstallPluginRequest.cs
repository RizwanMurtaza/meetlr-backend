namespace Meetlr.Api.Endpoints.Plugins.Models;

public record InstallPluginRequest(string PluginId, Dictionary<string, object>? Settings);
