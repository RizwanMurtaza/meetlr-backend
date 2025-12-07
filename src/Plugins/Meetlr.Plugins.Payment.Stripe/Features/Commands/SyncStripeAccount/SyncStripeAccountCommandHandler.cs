using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Payments;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stripe;

namespace Meetlr.Plugins.Payment.Stripe.Features.Commands.SyncStripeAccount;

/// <summary>
/// Syncs a Stripe Connect account's status from Stripe API
/// </summary>
public class SyncStripeAccountCommandHandler : IRequestHandler<SyncStripeAccountCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SyncStripeAccountCommandHandler> _logger;

    public SyncStripeAccountCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<SyncStripeAccountCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<bool> Handle(SyncStripeAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            StripeAccount? stripeAccount = null;

            // If StripeAccountId is provided, use it (webhook scenario)
            if (!string.IsNullOrEmpty(request.StripeAccountId))
            {
                stripeAccount = await _unitOfWork.Repository<StripeAccount>()
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.StripeAccountId == request.StripeAccountId, cancellationToken);
            }
            // Otherwise, use current user ID (manual sync scenario)
            else
            {
                var userId = _currentUserService.UserId ?? throw AuthenticationErrors.UserNotAuthenticated();

                stripeAccount = await _unitOfWork.Repository<StripeAccount>()
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
            }

            if (stripeAccount == null)
            {
                _logger.LogWarning("Stripe account not found in database");
                return false;
            }

            // Fetch latest account details from Stripe using the stored account ID
            var accountService = new AccountService();
            var account = await accountService.GetAsync(stripeAccount.StripeAccountId, cancellationToken: cancellationToken);

            // Update account status
            stripeAccount.ChargesEnabled = account.ChargesEnabled;
            stripeAccount.PayoutsEnabled = account.PayoutsEnabled;
            stripeAccount.DetailsSubmitted = account.DetailsSubmitted;
            stripeAccount.Country = account.Country;
            stripeAccount.Currency = account.DefaultCurrency;
            stripeAccount.Email = account.Email;
            stripeAccount.BusinessType = account.BusinessType;
            stripeAccount.LastSyncedAt = DateTime.UtcNow;
            stripeAccount.UpdatedAt = DateTime.UtcNow;

            // Update verification status
            if (account.Requirements?.CurrentlyDue?.Count > 0)
                stripeAccount.VerificationStatus = "pending";
            else if (account.Requirements?.Errors?.Count > 0)
                stripeAccount.VerificationStatus = "unverified";
            else
                stripeAccount.VerificationStatus = "verified";

            _unitOfWork.Repository<StripeAccount>().Update(stripeAccount);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Synced Stripe account {StripeAccountId} - ChargesEnabled: {ChargesEnabled}, PayoutsEnabled: {PayoutsEnabled}",
                request.StripeAccountId,
                account.ChargesEnabled,
                account.PayoutsEnabled
            );

            return true;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe API error syncing account {StripeAccountId}", request.StripeAccountId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing Stripe account {StripeAccountId}", request.StripeAccountId);
            return false;
        }
    }
}
