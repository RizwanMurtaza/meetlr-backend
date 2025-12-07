using Meetlr.Application.Plugins.Services;
using Meetlr.Module.Billing.Application.Interfaces;

namespace Meetlr.Module.Billing.Infrastructure.Services;

/// <summary>
/// Implementation of IUserBillingService that delegates to the CreditService.
/// Registered in DI to be called during user registration.
/// </summary>
public class UserBillingService : IUserBillingService
{
    private readonly ICreditService _creditService;

    public UserBillingService(ICreditService creditService)
    {
        _creditService = creditService;
    }

    public async Task AssignFreePackageAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await _creditService.AssignFreePackageAsync(userId, cancellationToken);
    }
}
