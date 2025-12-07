using System.Text.Json;
using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Plugins;
using Meetlr.Domain.Entities.Plugins;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Plugins.Commands.InstallPlugin;

public class InstallPluginCommandHandler : IRequestHandler<InstallPluginCommand, InstallPluginResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPluginFactory _pluginFactory;

    public InstallPluginCommandHandler(
        IUnitOfWork unitOfWork,
        IPluginFactory pluginFactory)
    {
        _unitOfWork = unitOfWork;
        _pluginFactory = pluginFactory;
    }

    public async Task<InstallPluginResponse> Handle(InstallPluginCommand request, CancellationToken cancellationToken)
    {
        // Find the plugin from plugin factory
        var plugin = _pluginFactory.GetPlugin(request.PluginId);
        if (plugin == null)
            throw PluginErrors.PluginNotFound(request.PluginId);

        if (!plugin.IsEnabled)
            throw PluginErrors.PluginNotAvailable(request.PluginId);

        // Check if already installed
        var existingPlugin = await _unitOfWork.Repository<UserInstalledPlugin>()
            .GetQueryable()
            .FirstOrDefaultAsync(p =>
                p.UserId == request.UserId &&
                p.PluginCategory == plugin.Category &&
                p.PluginId == plugin.PluginId
               , cancellationToken);

        if(existingPlugin!= null && existingPlugin.IsDeleted)
        {
            existingPlugin.IsDeleted = false;
            _unitOfWork.Repository<UserInstalledPlugin>().Update(existingPlugin);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new InstallPluginResponse
            {
                Success = true,
                Message = $"{plugin.PluginName} installed successfully",
                InstalledPluginId = existingPlugin.Id,
                PluginName = existingPlugin.PluginName,
                RequiresConnection = plugin.RequiresConnection
            };
        }

        if (existingPlugin != null)
            throw PluginErrors.PluginAlreadyInstalled(request.PluginId);

        // Install the plugin
        var userPlugin = new UserInstalledPlugin
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            PluginCategory = plugin.Category,
            PluginId = plugin.PluginId,
            PluginName = plugin.PluginName,
            PluginVersion = plugin.Version,
            IsEnabled = true,
            IsConnected = false,
            InstalledAt = DateTime.UtcNow,
            Settings = request.Settings != null ? JsonSerializer.Serialize(request.Settings) : null
        };

        _unitOfWork.Repository<UserInstalledPlugin>().Add(userPlugin);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new InstallPluginResponse
        {
            Success = true,
            Message = $"{plugin.PluginName} installed successfully",
            InstalledPluginId = userPlugin.Id,
            PluginName = plugin.PluginName,
            RequiresConnection = plugin.RequiresConnection
        };
    }
}
