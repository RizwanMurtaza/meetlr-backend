using Meetlr.Domain.Entities.Emails;

namespace Meetlr.Module.Notifications.Infrastructure.Services;

/// <summary>
/// Service for resolving email provider configuration with hierarchical fallback
/// Resolution order: User → Tenant → System
/// </summary>
public interface IEmailProviderConfigurationResolver
{
    /// <summary>
    /// Get the active email provider configuration for a specific provider type
    /// Returns the highest priority active configuration (User → Tenant → System)
    /// </summary>
    /// <param name="providerType">Provider type (SendGrid, Mailchimp, etc.)</param>
    /// <param name="tenantId">Tenant ID (optional)</param>
    /// <param name="userId">User ID (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Active email provider configuration or null if none found</returns>
    Task<EmailProviderConfiguration?> GetActiveConfigurationAsync(
        string providerType,
        Guid? tenantId = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active email provider configurations for all providers
    /// Returns configurations for all provider types in hierarchical order
    /// </summary>
    /// <param name="tenantId">Tenant ID (optional)</param>
    /// <param name="userId">User ID (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of provider type to configuration</returns>
    Task<Dictionary<string, EmailProviderConfiguration>> GetAllActiveConfigurationsAsync(
        Guid? tenantId = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default);
}
