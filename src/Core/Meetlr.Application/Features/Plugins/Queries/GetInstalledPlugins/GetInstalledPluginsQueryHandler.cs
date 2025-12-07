using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Plugins;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Plugins.Queries.GetInstalledPlugins;

public class GetInstalledPluginsQueryHandler : IRequestHandler<GetInstalledPluginsQuery, GetInstalledPluginsResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetInstalledPluginsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetInstalledPluginsResponse> Handle(GetInstalledPluginsQuery request, CancellationToken cancellationToken)
    {
        var installedPlugins = await _unitOfWork.Repository<UserInstalledPlugin>()
            .GetQueryable()
            .Where(p => p.UserId == request.UserId && !p.IsDeleted)
            .OrderByDescending(p => p.InstalledAt)
            .ToListAsync(cancellationToken);

        var plugins = installedPlugins.Select(p => new InstalledPluginInfo
        {
            Id = p.Id,
            PluginId = p.PluginId,
            PluginName = p.PluginName,
            Category = p.PluginCategory,
            Version = p.PluginVersion!,
            IsEnabled = p.IsEnabled,
            IsConnected = p.IsConnected,
            ConnectionStatus = p.ConnectionStatus!,
            InstalledAt = p.InstalledAt,
            ConnectedAt = p.ConnectedAt,
            LastUsedAt = p.LastUsedAt,
            UsageCount = p.UsageCount,
            ErrorMessage = p.ErrorMessage
        }).ToList();

        return new GetInstalledPluginsResponse
        {
            Plugins = plugins
        };
    }
}
