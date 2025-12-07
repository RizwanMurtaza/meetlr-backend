using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Plugins;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Plugins.Commands.DisablePlugin;

public class DisablePluginCommandHandler : IRequestHandler<DisablePluginCommand, DisablePluginResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public DisablePluginCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DisablePluginResponse> Handle(DisablePluginCommand request, CancellationToken cancellationToken)
    {
        var userPlugin = await _unitOfWork.Repository<UserInstalledPlugin>()
            .GetQueryable()
            .FirstOrDefaultAsync(p =>
                p.UserId == request.UserId &&
                p.PluginId == request.PluginId &&
                !p.IsDeleted, cancellationToken);

        if (userPlugin == null)
            throw PluginErrors.PluginNotInstalled(request.PluginId);

        userPlugin.IsEnabled = false;
        userPlugin.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DisablePluginResponse
        {
            Success = true,
            Message = "Plugin disabled successfully"
        };
    }
}
