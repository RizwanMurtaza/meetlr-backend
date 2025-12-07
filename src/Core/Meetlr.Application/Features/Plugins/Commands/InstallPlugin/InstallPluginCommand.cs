using MediatR;

namespace Meetlr.Application.Features.Plugins.Commands.InstallPlugin;

public class InstallPluginCommand : IRequest<InstallPluginResponse>
{
    public Guid UserId { get; set; }
    public string PluginId { get; set; } = string.Empty;
    public Dictionary<string, object>? Settings { get; set; }
}