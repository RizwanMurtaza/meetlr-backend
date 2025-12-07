using MediatR;

namespace Meetlr.Application.Features.Plugins.Commands.UninstallPlugin;

public class UninstallPluginCommand : IRequest<UninstallPluginResponse>
{
    public Guid UserId { get; set; }
    public string PluginId { get; set; } = string.Empty;
}