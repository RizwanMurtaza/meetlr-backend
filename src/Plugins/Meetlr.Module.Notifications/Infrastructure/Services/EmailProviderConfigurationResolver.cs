using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Emails;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Module.Notifications.Infrastructure.Services;

/// <summary>
/// Service for resolving email provider configuration with hierarchical fallback
/// Resolution order: User → Tenant → System
/// </summary>
public class EmailProviderConfigurationResolver : IEmailProviderConfigurationResolver
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EmailProviderConfigurationResolver> _logger;

    public EmailProviderConfigurationResolver(
        IUnitOfWork unitOfWork,
        ILogger<EmailProviderConfigurationResolver> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<EmailProviderConfiguration?> GetActiveConfigurationAsync(
        string providerType,
        Guid? tenantId = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Resolving {ProviderType} configuration for TenantId: {TenantId}, UserId: {UserId}",
            providerType, tenantId, userId);

        // 1. User-level active configuration (highest priority)
        if (userId.HasValue)
        {
            var userConfig = await _unitOfWork.Repository<EmailProviderConfiguration>()
                .GetQueryable()
                .Where(c =>
                    c.ProviderType == providerType &&
                    c.UserId == userId &&
                    c.IsActive &&
                    !c.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (userConfig != null)
            {
                _logger.LogDebug("Found user-level {ProviderType} configuration: {FromEmail}",
                    providerType, userConfig.DefaultFromEmail);
                return userConfig;
            }
        }

        // 2. Tenant-level active configuration (medium priority)
        if (tenantId.HasValue)
        {
            var tenantConfig = await _unitOfWork.Repository<EmailProviderConfiguration>()
                .GetQueryable()
                .Where(c =>
                    c.ProviderType == providerType &&
                    c.TenantId == tenantId &&
                    c.UserId == null &&
                    c.IsActive &&
                    !c.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (tenantConfig != null)
            {
                _logger.LogDebug("Found tenant-level {ProviderType} configuration: {FromEmail}",
                    providerType, tenantConfig.DefaultFromEmail);
                return tenantConfig;
            }
        }

        // 3. System-level configuration (always fallback, lowest priority)
        var systemConfig = await _unitOfWork.Repository<EmailProviderConfiguration>()
            .GetQueryable()
            .Where(c =>
                c.ProviderType == providerType &&
                c.IsSystemDefault &&
                c.IsActive &&
                !c.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (systemConfig != null)
        {
            _logger.LogDebug("Found system-level {ProviderType} configuration: {FromEmail}",
                providerType, systemConfig.DefaultFromEmail);
            return systemConfig;
        }

        _logger.LogWarning("No {ProviderType} configuration found at any level for TenantId: {TenantId}, UserId: {UserId}",
            providerType, tenantId, userId);

        return null;
    }

    public async Task<Dictionary<string, EmailProviderConfiguration>> GetAllActiveConfigurationsAsync(
        Guid? tenantId = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var configurations = new Dictionary<string, EmailProviderConfiguration>();

        // Get all unique provider types
        var providerTypes = await _unitOfWork.Repository<EmailProviderConfiguration>()
            .GetQueryable()
            .Where(c => c.IsActive && !c.IsDeleted)
            .Select(c => c.ProviderType)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var providerType in providerTypes)
        {
            var config = await GetActiveConfigurationAsync(providerType, tenantId, userId, cancellationToken);
            if (config != null)
            {
                configurations[providerType] = config;
            }
        }

        _logger.LogInformation("Retrieved {Count} active provider configurations", configurations.Count);

        return configurations;
    }
}
