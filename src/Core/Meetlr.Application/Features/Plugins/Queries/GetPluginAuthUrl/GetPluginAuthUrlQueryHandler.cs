using MediatR;
using Meetlr.Application.Plugins;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;

namespace Meetlr.Application.Features.Plugins.Queries.GetPluginAuthUrl;

public class GetPluginAuthUrlQueryHandler : IRequestHandler<GetPluginAuthUrlQuery, GetPluginAuthUrlResponse>
{
    private readonly IEnumerable<IPlugin> _plugins;

    public GetPluginAuthUrlQueryHandler(IEnumerable<IPlugin> plugins)
    {
        _plugins = plugins;
    }

    public Task<GetPluginAuthUrlResponse> Handle(GetPluginAuthUrlQuery request, CancellationToken cancellationToken)
    {
        var plugin = _plugins.FirstOrDefault(p => p.PluginId.Equals(request.PluginId, StringComparison.OrdinalIgnoreCase));

        if (plugin == null)
        {
            throw PluginErrors.PluginNotFound(request.PluginId);
        }

        if (!plugin.RequiresConnection)
        {
            throw PluginErrors.PluginNotFound(request.PluginId); // Plugin doesn't require OAuth
        }

        // Generate OAuth authorization URL
        // State format: pluginId:userId - this allows callback to identify both
        var state = $"{request.PluginId}:{request.UserId}";
        var authUrl = plugin.GenerateConnectUrlAsync(request.UserId, request.RedirectUri, cancellationToken).Result;

        if (string.IsNullOrEmpty(authUrl))
        {
            throw new InvalidOperationException($"Plugin {request.PluginId} did not return an authorization URL");
        }

        return Task.FromResult(new GetPluginAuthUrlResponse
        {
            AuthorizationUrl = authUrl
        });
    }
}
