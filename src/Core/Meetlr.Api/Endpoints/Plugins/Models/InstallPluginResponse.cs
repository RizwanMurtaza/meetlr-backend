namespace Meetlr.Api.Endpoints.Plugins.Models;

public record InstallPluginResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = default!;
    public string PluginId { get; init; } = default!;
    public bool RequiresConnection { get; init; }
}
