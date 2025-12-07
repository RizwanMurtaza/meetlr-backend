using MediatR;

namespace Meetlr.Application.Features.Plugins.Queries.GetInstalledPlugins;

public class GetInstalledPluginsQuery : IRequest<GetInstalledPluginsResponse>
{
    public Guid UserId { get; set; }
}