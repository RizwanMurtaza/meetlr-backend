using MediatR;

namespace Meetlr.Application.Features.Plugins.Queries.GetAvailablePlugins;

public class GetAvailablePluginsQuery : IRequest<GetAvailablePluginsResponse>
{
    public Guid UserId { get; set; }
}