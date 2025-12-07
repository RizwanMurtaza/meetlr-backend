using MediatR;

namespace Meetlr.Application.Features.Plugins.Commands.ConnectPlugin;

public class ConnectPluginCommand : IRequest<ConnectPluginResponse>
{
    public Guid UserId { get; set; }
    public string PluginId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
}
