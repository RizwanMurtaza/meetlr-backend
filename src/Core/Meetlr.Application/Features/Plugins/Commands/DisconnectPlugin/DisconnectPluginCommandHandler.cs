using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Plugins;
using Meetlr.Domain.Entities.Plugins;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Plugins.Commands.DisconnectPlugin;

public class DisconnectPluginCommandHandler : IRequestHandler<DisconnectPluginCommand, DisconnectPluginResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnumerable<IPlugin> _plugins;

    public DisconnectPluginCommandHandler(
        IUnitOfWork unitOfWork,
        IEnumerable<IPlugin> plugins)
    {
        _unitOfWork = unitOfWork;
        _plugins = plugins;
    }

    public async Task<DisconnectPluginResponse> Handle(DisconnectPluginCommand request, CancellationToken cancellationToken)
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

        // Disconnect through the plugin (revoke tokens if needed)
        await plugin.DisconnectAsync(request.UserId, cancellationToken);

        // Clear connection data from the installed plugin record
        installedPlugin.IsConnected = false;
        installedPlugin.ConnectedAt = null;
        installedPlugin.ConnectionStatus = null;
        installedPlugin.AccessToken = null;
        installedPlugin.RefreshToken = null;
        installedPlugin.TokenExpiresAt = null;
        installedPlugin.ProviderEmail = null;
        installedPlugin.ProviderId = null;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DisconnectPluginResponse
        {
            Success = true,
            Message = $"{plugin.PluginName} disconnected successfully"
        };
    }
}
