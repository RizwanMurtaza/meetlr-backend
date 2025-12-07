using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Plugins;
using Meetlr.Application.Plugins.MeetingTypes;
using Meetlr.Application.Plugins.Services;
using Meetlr.Domain.Entities.Plugins;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Infrastructure.Services;

/// <summary>
/// Service for automatically installing plugins based on user actions
/// </summary>
public class PluginAutoInstallService : IPluginAutoInstallService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPluginFactory _pluginFactory;
    private readonly ILogger<PluginAutoInstallService> _logger;

    public PluginAutoInstallService(
        IUnitOfWork unitOfWork,
        IPluginFactory pluginFactory,
        ILogger<PluginAutoInstallService> logger)
    {
        _unitOfWork = unitOfWork;
        _pluginFactory = pluginFactory;
        _logger = logger;
    }

    public async Task AutoInstallPluginsForOAuthProviderAsync(
        Guid userId,
        string oauthProvider,
        string? accessToken = null,
        string? refreshToken = null,
        DateTime? tokenExpiry = null,
        string? providerEmail = null,
        CancellationToken cancellationToken = default)
    {
        // Get the plugin ID to auto-install for this OAuth provider
        var pluginId = MeetingTypePluginMetadata.GetAutoInstallPluginId(oauthProvider);

        if (string.IsNullOrEmpty(pluginId))
        {
            _logger.LogDebug(
                "No meeting type plugin configured for auto-install on OAuth provider {Provider}",
                oauthProvider);
            return;
        }

        _logger.LogInformation(
            "Auto-installing meeting type plugin {PluginId} for user {UserId} after {Provider} OAuth sign-in",
            pluginId, userId, oauthProvider);

        // Install the plugin and pass OAuth tokens so it's automatically connected
        var hasTokens = !string.IsNullOrEmpty(accessToken);
        var installed = await InstallPluginAsync(
            userId,
            pluginId,
            markAsConnected: hasTokens,
            accessToken: accessToken,
            refreshToken: refreshToken,
            tokenExpiry: tokenExpiry,
            providerEmail: providerEmail,
            cancellationToken: cancellationToken);

        if (installed)
        {
            _logger.LogInformation(
                "Successfully auto-installed plugin {PluginId} for user {UserId} (connected: {IsConnected})",
                pluginId, userId, hasTokens);
        }
        else
        {
            _logger.LogDebug(
                "Plugin {PluginId} already installed for user {UserId} or plugin not found",
                pluginId, userId);
        }
    }

    public async Task<bool> InstallPluginAsync(
        Guid userId,
        string pluginId,
        bool markAsConnected = false,
        string? accessToken = null,
        string? refreshToken = null,
        DateTime? tokenExpiry = null,
        string? providerEmail = null,
        CancellationToken cancellationToken = default)
    {
        // Get the plugin from the factory
        var plugin = _pluginFactory.GetPlugin(pluginId);

        if (plugin == null)
        {
            _logger.LogWarning("Plugin {PluginId} not found in plugin factory", pluginId);
            return false;
        }

        // Check if plugin is already installed for this user
        var existingPlugin = await _unitOfWork.Repository<UserInstalledPlugin>()
            .GetQueryable()
            .FirstOrDefaultAsync(
                p => p.UserId == userId &&
                     p.PluginId == pluginId &&
                     !p.IsDeleted,
                cancellationToken);

        if (existingPlugin != null)
        {
            // Plugin already installed, update connection status and tokens if needed
            var needsUpdate = false;

            if (markAsConnected && !existingPlugin.IsConnected)
            {
                existingPlugin.IsConnected = true;
                existingPlugin.ConnectedAt = DateTime.UtcNow;
                existingPlugin.ConnectionStatus = "connected";
                existingPlugin.IsEnabled = true;
                needsUpdate = true;
            }

            // Update OAuth tokens if provided
            if (!string.IsNullOrEmpty(accessToken))
            {
                existingPlugin.AccessToken = accessToken;
                existingPlugin.RefreshToken = refreshToken;
                existingPlugin.TokenExpiresAt = tokenExpiry;
                existingPlugin.ProviderEmail = providerEmail;
                existingPlugin.IsConnected = true;
                existingPlugin.ConnectedAt ??= DateTime.UtcNow;
                existingPlugin.ConnectionStatus = "connected";
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogDebug(
                    "Updated existing plugin {PluginId} for user {UserId} with new tokens/status",
                    pluginId, userId);
            }

            return false; // Already installed (but may have been updated)
        }

        // Check if there's a soft-deleted installation we can restore
        var deletedPlugin = await _unitOfWork.Repository<UserInstalledPlugin>()
            .GetQueryable()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                p => p.UserId == userId &&
                     p.PluginId == pluginId &&
                     p.IsDeleted,
                cancellationToken);

        if (deletedPlugin != null)
        {
            // Restore the deleted plugin
            deletedPlugin.IsDeleted = false;
            deletedPlugin.DeletedAt = null;
            deletedPlugin.DeletedBy = null;
            deletedPlugin.IsEnabled = true;
            deletedPlugin.IsConnected = markAsConnected || !string.IsNullOrEmpty(accessToken);
            deletedPlugin.InstalledAt = DateTime.UtcNow;
            deletedPlugin.ConnectionStatus = deletedPlugin.IsConnected ? "connected" : "pending";
            deletedPlugin.ErrorMessage = null;

            if (deletedPlugin.IsConnected)
            {
                deletedPlugin.ConnectedAt = DateTime.UtcNow;
            }

            // Set OAuth tokens if provided
            if (!string.IsNullOrEmpty(accessToken))
            {
                deletedPlugin.AccessToken = accessToken;
                deletedPlugin.RefreshToken = refreshToken;
                deletedPlugin.TokenExpiresAt = tokenExpiry;
                deletedPlugin.ProviderEmail = providerEmail;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogDebug(
                "Restored previously deleted plugin {PluginId} for user {UserId}",
                pluginId, userId);

            return true;
        }

        // Create new installation
        var isConnected = markAsConnected || !string.IsNullOrEmpty(accessToken);
        var newPlugin = new UserInstalledPlugin
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PluginCategory = plugin.Category,
            PluginId = plugin.PluginId,
            PluginName = plugin.PluginName,
            PluginVersion = plugin.Version,
            IsEnabled = true,
            IsConnected = isConnected,
            ConnectionStatus = isConnected ? "connected" : "pending",
            InstalledAt = DateTime.UtcNow,
            ConnectedAt = isConnected ? DateTime.UtcNow : null,
            // Store OAuth tokens if provided
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenExpiresAt = tokenExpiry,
            ProviderEmail = providerEmail
        };

        _unitOfWork.Repository<UserInstalledPlugin>().Add(newPlugin);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Created new plugin installation {PluginId} for user {UserId} (connected: {IsConnected})",
            pluginId, userId, isConnected);

        return true;
    }
}
