namespace Meetlr.Api.Endpoints.Plugins.Models;

public class PluginConnectRequest
{
    public string Code { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
}
