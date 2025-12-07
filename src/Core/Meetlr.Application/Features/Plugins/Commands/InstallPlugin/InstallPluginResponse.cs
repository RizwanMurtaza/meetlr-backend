namespace Meetlr.Application.Features.Plugins.Commands.InstallPlugin;

public class InstallPluginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? InstalledPluginId { get; set; }
    public string PluginName { get; set; } = string.Empty;
    public bool RequiresConnection { get; set; }
}