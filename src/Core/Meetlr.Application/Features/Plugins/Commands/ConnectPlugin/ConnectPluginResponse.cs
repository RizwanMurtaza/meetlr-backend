namespace Meetlr.Application.Features.Plugins.Commands.ConnectPlugin;

public class ConnectPluginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Email { get; set; }
}
