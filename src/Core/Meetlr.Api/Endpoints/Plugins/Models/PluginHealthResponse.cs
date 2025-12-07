namespace Meetlr.Api.Endpoints.Plugins.Models;

public record PluginHealthResponse
{
    public string PluginId { get; init; } = default!;
    public string PluginName { get; init; } = default!;
    public bool IsHealthy { get; init; }
    public string Status { get; init; } = default!;
    public string? Message { get; init; }
    public DateTime LastChecked { get; init; }
    public Dictionary<string, object>? Details { get; init; }
}
