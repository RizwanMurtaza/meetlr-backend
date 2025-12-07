using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Plugins;
using Meetlr.Domain.Entities.Plugins;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Plugins.Queries.GetAvailablePlugins;

public class GetAvailablePluginsQueryHandler : IRequestHandler<GetAvailablePluginsQuery, GetAvailablePluginsResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPluginFactory _pluginFactory;

    public GetAvailablePluginsQueryHandler(
        IUnitOfWork unitOfWork,
        IPluginFactory pluginFactory)
    {
        _unitOfWork = unitOfWork;
        _pluginFactory = pluginFactory;
    }

    public async Task<GetAvailablePluginsResponse> Handle(GetAvailablePluginsQuery request, CancellationToken cancellationToken)
    {
        // Get user's installed plugins
        var installedPlugins = await _unitOfWork.Repository<UserInstalledPlugin>()
            .GetQueryable()
            .Where(p => p.UserId == request.UserId && !p.IsDeleted)
            .ToListAsync(cancellationToken);

        var plugins = new List<PluginInfo>();

        // Get all plugins from unified factory
        var allPlugins = _pluginFactory.GetAllPlugins();
        foreach (var plugin in allPlugins)
        {
            var userPlugin = installedPlugins.FirstOrDefault(p =>
                p.PluginCategory == plugin.Category &&
                p.PluginId == plugin.PluginId);

            var health = await plugin.GetHealthStatusAsync(cancellationToken);

            // Check connection status for plugins that require connection
            var connectionStatus = plugin.RequiresConnection
                ? await plugin.GetConnectionStatusAsync(request.UserId, cancellationToken)
                : null;

            plugins.Add(new PluginInfo
            {
                PluginId = plugin.PluginId,
                PluginName = plugin.PluginName,
                Category = plugin.Category,
                Description = plugin.Description,
                Version = plugin.Version,
                Author = plugin.Author ?? string.Empty,
                IconUrl = plugin.IconUrl,
                RequiresConnection = plugin.RequiresConnection,
                IsAvailable = plugin.IsEnabled,
                IsInstalled = userPlugin != null,
                IsEnabled = userPlugin?.IsEnabled ?? false,
                IsConnected = connectionStatus?.IsConnected ?? userPlugin?.IsConnected ?? false,
                ConnectionStatus = connectionStatus?.IsConnected == true ? "connected" : (userPlugin?.ConnectionStatus ?? string.Empty),
                InstalledAt = userPlugin?.InstalledAt,
                ConnectedAt = connectionStatus?.ConnectedAt ?? userPlugin?.ConnectedAt,
                LastUsedAt = userPlugin?.LastUsedAt,
                UsageCount = userPlugin?.UsageCount ?? 0,
                HealthStatus = health.Status,
                HealthMessage = health.Message
            });
        }

        return new GetAvailablePluginsResponse
        {
            Plugins = plugins
        };
    }
}
