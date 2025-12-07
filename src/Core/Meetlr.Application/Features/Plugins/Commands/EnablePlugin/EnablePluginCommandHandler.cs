using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Plugins;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Plugins.Commands.EnablePlugin;

public class EnablePluginCommandHandler : IRequestHandler<EnablePluginCommand, EnablePluginResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public EnablePluginCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<EnablePluginResponse> Handle(EnablePluginCommand request, CancellationToken cancellationToken)
    {
        var userPlugin = await _unitOfWork.Repository<UserInstalledPlugin>()
            .GetQueryable()
            .FirstOrDefaultAsync(p =>
                p.UserId == request.UserId &&
                p.PluginId == request.PluginId &&
                !p.IsDeleted, cancellationToken);

        if (userPlugin == null)
            throw PluginErrors.PluginNotInstalled(request.PluginId);

        userPlugin.IsEnabled = true;
        userPlugin.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new EnablePluginResponse
        {
            Success = true,
            Message = "Plugin enabled successfully"
        };
    }
}
