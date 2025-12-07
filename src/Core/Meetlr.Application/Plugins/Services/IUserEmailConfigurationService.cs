namespace Meetlr.Application.Plugins.Services;

/// <summary>
/// Service for user email configuration operations during signup/registration.
/// Implemented by the Notifications module.
/// Seeds default SMTP configuration for new users.
/// </summary>
public interface IUserEmailConfigurationService
{
    /// <summary>
    /// Creates a default SMTP email configuration for a new user.
    /// Uses the system Oracle Email Delivery Service credentials.
    /// </summary>
    /// <param name="userId">The ID of the newly registered user</param>
    /// <param name="tenantId">The ID of the user's tenant</param>
    /// <param name="userEmail">The user's email address (used as FromEmail)</param>
    /// <param name="userName">The user's display name (used as FromName)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CreateDefaultEmailConfigurationAsync(
        Guid userId,
        Guid tenantId,
        string userEmail,
        string userName,
        CancellationToken cancellationToken = default);
}
