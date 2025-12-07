namespace Meetlr.Api.Endpoints.Plugins.Models;

public record PluginInfoResponse
{
    public string PluginId { get; init; } = default!;
    public string PluginName { get; init; } = default!;
    public string Category { get; init; } = default!;
    public string Description { get; init; } = default!;
    public string Version { get; init; } = default!;
    public string? Author { get; init; }
    public string? IconUrl { get; init; }
    public bool RequiresConnection { get; init; }
    public bool IsAvailable { get; init; }
    public bool IsInstalled { get; init; }
    public bool IsEnabled { get; init; }
    public bool IsConnected { get; init; }
    public string? ConnectionStatus { get; init; }
    public DateTime? InstalledAt { get; init; }
    public DateTime? ConnectedAt { get; init; }
    public DateTime? LastUsedAt { get; init; }
    public int UsageCount { get; init; }
    public string HealthStatus { get; init; } = default!;
    public string? HealthMessage { get; init; }
}
