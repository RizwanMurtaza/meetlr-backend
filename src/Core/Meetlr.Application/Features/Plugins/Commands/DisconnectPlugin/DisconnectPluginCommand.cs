using MediatR;

namespace Meetlr.Application.Features.Plugins.Commands.DisconnectPlugin;

public class DisconnectPluginCommand : IRequest<DisconnectPluginResponse>
{
    public Guid UserId { get; set; }
    public string PluginId { get; set; } = string.Empty;
}
