namespace Meetlr.Application.Plugins.Models;

/// <summary>
/// Plugin health status
/// </summary>
public class PluginHealthStatus
{
    public bool IsHealthy { get; set; }
    public string Status { get; set; } = "Unknown";
    public string? Message { get; set; }
    public Dictionary<string, string> Details { get; set; } = new();
}
