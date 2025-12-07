using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Plugins;
using Meetlr.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Plugins.Queries.ValidatePaymentPlugin;

public class ValidatePaymentPluginQueryHandler : IRequestHandler<ValidatePaymentPluginQuery, ValidatePaymentPluginResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public ValidatePaymentPluginQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ValidatePaymentPluginResponse> Handle(ValidatePaymentPluginQuery request, CancellationToken cancellationToken)
    {
        // Verify the selected payment provider is installed and connected for this user
        var hasPaymentPlugin = await _unitOfWork.Repository<UserInstalledPlugin>()
            .GetQueryable()
            .AnyAsync(p =>
                p.UserId == request.UserId &&
                p.PluginId == request.PaymentProviderType &&
                p.PluginCategory == PluginCategory.Payment &&
                p.IsEnabled &&
                p.IsConnected &&
                !p.IsDeleted, cancellationToken);

        if (!hasPaymentPlugin)
        {
            return new ValidatePaymentPluginResponse
            {
                IsValid = false,
                ErrorMessage = $"Payment plugin '{request.PaymentProviderType}' is not installed or not connected"
            };
        }

        return new ValidatePaymentPluginResponse
        {
            IsValid = true
        };
    }
}
