using MediatR;

namespace Meetlr.Application.Features.Plugins.Queries.GetPluginAuthUrl;

public class GetPluginAuthUrlQuery : IRequest<GetPluginAuthUrlResponse>
{
    public Guid UserId { get; set; }
    public string PluginId { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
}
