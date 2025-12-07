using Meetlr.Domain.Enums;

namespace Meetlr.Application.Features.Plugins.Queries.GetAvailablePlugins;

public class PluginInfo
{
    public string PluginId { get; set; } = string.Empty;
    public string PluginName { get; set; } = string.Empty;
    public PluginCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public bool RequiresConnection { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsInstalled { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsConnected { get; set; }
    public string? ConnectionStatus { get; set; }
    public DateTime? InstalledAt { get; set; }
    public DateTime? ConnectedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public int UsageCount { get; set; }
    public string HealthStatus { get; set; } = string.Empty;
    public string? HealthMessage { get; set; }
}