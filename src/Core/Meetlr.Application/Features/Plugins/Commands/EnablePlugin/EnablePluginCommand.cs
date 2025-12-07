using MediatR;

namespace Meetlr.Application.Features.Plugins.Commands.EnablePlugin;

public class EnablePluginCommand : IRequest<EnablePluginResponse>
{
    public Guid UserId { get; set; }
    public string PluginId { get; set; } = string.Empty;
}