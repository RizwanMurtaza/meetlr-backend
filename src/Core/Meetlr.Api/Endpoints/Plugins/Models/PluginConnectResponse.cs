namespace Meetlr.Api.Endpoints.Plugins.Models;

public class PluginConnectResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string PluginId { get; set; } = string.Empty;
    public string? Email { get; set; }
}
