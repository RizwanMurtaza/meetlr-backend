using MediatR;

namespace Meetlr.Application.Features.Plugins.Commands.DisablePlugin;

public class DisablePluginCommand : IRequest<DisablePluginResponse>
{
    public Guid UserId { get; set; }
    public string PluginId { get; set; } = string.Empty;
}