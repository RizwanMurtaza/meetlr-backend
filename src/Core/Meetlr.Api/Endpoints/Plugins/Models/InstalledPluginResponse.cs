namespace Meetlr.Api.Endpoints.Plugins.Models;

public record InstalledPluginResponse
{
    public Guid Id { get; init; }
    public string PluginId { get; init; } = default!;
    public string PluginName { get; init; } = default!;
    public string Category { get; init; } = default!;
    public string? Version { get; init; }
    public bool IsEnabled { get; init; }
    public bool IsConnected { get; init; }
    public string? ConnectionStatus { get; init; }
    public DateTime InstalledAt { get; init; }
    public DateTime? ConnectedAt { get; init; }
    public DateTime? LastUsedAt { get; init; }
    public int UsageCount { get; init; }
    public string? ErrorMessage { get; init; }
}
