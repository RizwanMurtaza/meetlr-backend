using Meetlr.Domain.Entities.Emails;

namespace Meetlr.Application.Interfaces;

/// <summary>
/// Service for resolving SMTP configuration with hierarchical fallback
/// Resolution order: User → Tenant → System
/// </summary>
public interface ISmtpConfigurationResolver
{
    /// <summary>
    /// Get SMTP configuration hierarchy for failover support
    /// Returns configurations in priority order: User → Tenant → System
    /// </summary>
    /// <param name="tenantId">Tenant ID (optional)</param>
    /// <param name="userId">User ID (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of SMTP configurations in priority order for failover</returns>
    Task<List<EmailConfiguration>> GetSmtpHierarchyAsync(
        Guid? tenantId = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the active SMTP configuration for a user/tenant
    /// Returns the highest priority active configuration
    /// </summary>
    /// <param name="tenantId">Tenant ID (optional)</param>
    /// <param name="userId">User ID (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Active SMTP configuration or null if none found</returns>
    Task<EmailConfiguration?> GetActiveSmtpConfigurationAsync(
        Guid? tenantId = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default);
}
