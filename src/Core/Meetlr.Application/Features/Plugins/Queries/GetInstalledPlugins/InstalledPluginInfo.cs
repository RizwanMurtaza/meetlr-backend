using Meetlr.Domain.Enums;

namespace Meetlr.Application.Features.Plugins.Queries.GetInstalledPlugins;

public class InstalledPluginInfo
{
    public Guid Id { get; set; }
    public string PluginId { get; set; } = string.Empty;
    public string PluginName { get; set; } = string.Empty;
    public PluginCategory Category { get; set; }
    public string Version { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public bool IsConnected { get; set; }
    public string? ConnectionStatus { get; set; }
    public DateTime InstalledAt { get; set; }
    public DateTime? ConnectedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public int UsageCount { get; set; }
    public string? ErrorMessage { get; set; }
}