namespace Meetlr.Application.Plugins.Services;

/// <summary>
/// Service for automatically installing plugins based on user actions (e.g., OAuth sign-in)
/// </summary>
public interface IPluginAutoInstallService
{
    /// <summary>
    /// Auto-install plugins that should be installed when a user signs in with a specific OAuth provider.
    /// Also stores the OAuth tokens so the plugin can use the same account.
    /// For example: Google OAuth -> Google Meet plugin, Microsoft OAuth -> Teams plugin
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="oauthProvider">The OAuth provider used (e.g., "Google", "Microsoft")</param>
    /// <param name="accessToken">The OAuth access token</param>
    /// <param name="refreshToken">The OAuth refresh token</param>
    /// <param name="tokenExpiry">When the access token expires</param>
    /// <param name="providerEmail">The email from the OAuth provider</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AutoInstallPluginsForOAuthProviderAsync(
        Guid userId,
        string oauthProvider,
        string? accessToken = null,
        string? refreshToken = null,
        DateTime? tokenExpiry = null,
        string? providerEmail = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Install a specific plugin for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="pluginId">The plugin ID to install</param>
    /// <param name="markAsConnected">Whether to mark the plugin as connected (e.g., for OAuth-based auto-install)</param>
    /// <param name="accessToken">Optional OAuth access token</param>
    /// <param name="refreshToken">Optional OAuth refresh token</param>
    /// <param name="tokenExpiry">Optional token expiry time</param>
    /// <param name="providerEmail">Optional provider email</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if installed successfully, false if already installed or plugin not found</returns>
    Task<bool> InstallPluginAsync(
        Guid userId,
        string pluginId,
        bool markAsConnected = false,
        string? accessToken = null,
        string? refreshToken = null,
        DateTime? tokenExpiry = null,
        string? providerEmail = null,
        CancellationToken cancellationToken = default);
}
