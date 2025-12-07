namespace Meetlr.Application.Plugins.Services;

/// <summary>
/// Service for user billing operations during signup/registration.
/// Implemented by the Billing module if installed.
/// </summary>
public interface IUserBillingService
{
    /// <summary>
    /// Assigns the free starter package to a new user.
    /// Called during user registration to grant initial credits.
    /// </summary>
    /// <param name="userId">The ID of the newly registered user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AssignFreePackageAsync(Guid userId, CancellationToken cancellationToken = default);
}
