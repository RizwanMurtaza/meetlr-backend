using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Plugins;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Plugins.Commands.UninstallPlugin;

public class UninstallPluginCommandHandler : IRequestHandler<UninstallPluginCommand, UninstallPluginResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public UninstallPluginCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UninstallPluginResponse> Handle(UninstallPluginCommand request, CancellationToken cancellationToken)
    {
        var userPlugin = await _unitOfWork.Repository<UserInstalledPlugin>()
            .GetQueryable()
            .FirstOrDefaultAsync(p =>
                p.UserId == request.UserId &&
                p.PluginId == request.PluginId &&
                !p.IsDeleted, cancellationToken);

        if (userPlugin == null)
            throw PluginErrors.PluginNotInstalled(request.PluginId);

        // Soft delete
        userPlugin.IsDeleted = true;
        userPlugin.DeletedAt = DateTime.UtcNow;
        userPlugin.DeletedBy = request.UserId.ToString();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UninstallPluginResponse
        {
            Success = true,
            Message = "Plugin uninstalled successfully"
        };
    }
}
