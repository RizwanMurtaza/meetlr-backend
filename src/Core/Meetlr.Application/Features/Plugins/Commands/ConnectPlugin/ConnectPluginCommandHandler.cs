using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Plugins;
using Meetlr.Domain.Entities.Plugins;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Plugins.Commands.ConnectPlugin;

public class ConnectPluginCommandHandler : IRequestHandler<ConnectPluginCommand, ConnectPluginResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnumerable<IPlugin> _plugins;

    public ConnectPluginCommandHandler(
        IUnitOfWork unitOfWork,
        IEnumerable<IPlugin> plugins)
    {
        _unitOfWork = unitOfWork;
        _plugins = plugins;
    }

    public async Task<ConnectPluginResponse> Handle(ConnectPluginCommand request, CancellationToken cancellationToken)
    {
        var plugin = _plugins.FirstOrDefault(p => p.PluginId.Equals(request.PluginId, StringComparison.OrdinalIgnoreCase));

        if (plugin == null)
        {
            throw PluginErrors.PluginNotFound(request.PluginId);
        }

        // Get the user's installed plugin record
        var installedPlugin = await _unitOfWork.Repository<UserInstalledPlugin>()
            .GetQueryable()
            .FirstOrDefaultAsync(p => p.UserId == request.UserId &&
                                      p.PluginId == request.PluginId &&
                                      !p.IsDeleted, cancellationToken);

        if (installedPlugin == null)
        {
            throw PluginErrors.PluginNotInstalled(request.PluginId);
        }

        // Complete the OAuth connection through the plugin
        // The plugin implementation will handle the token exchange and storage
        var success = await plugin.CompleteConnectAsync(request.Code, request.RedirectUri, request.UserId, cancellationToken);

        if (!success)
        {
            return new ConnectPluginResponse
            {
                Success = false,
                Message = "Failed to connect plugin"
            };
        }

        // Get the connection status to retrieve email
        var connectionStatus = await plugin.GetConnectionStatusAsync(request.UserId, cancellationToken);

        // Update the installed plugin record
        installedPlugin.IsConnected = true;
        installedPlugin.ConnectedAt = DateTime.UtcNow;
        installedPlugin.ConnectionStatus = "connected";
        installedPlugin.ErrorMessage = null;
        installedPlugin.ProviderEmail = connectionStatus?.Email;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ConnectPluginResponse
        {
            Success = true,
            Message = $"{plugin.PluginName} connected successfully",
            Email = connectionStatus?.Email
        };
    }
}
