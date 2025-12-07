using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Emails;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Module.Notifications.Infrastructure.Services;

/// <summary>
/// Service for resolving SMTP configuration with hierarchical fallback
/// Resolution order: User → Tenant → System
/// Supports failover by returning all configs in priority order
/// </summary>
public class SmtpConfigurationResolver : ISmtpConfigurationResolver
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SmtpConfigurationResolver> _logger;

    public SmtpConfigurationResolver(
        IUnitOfWork unitOfWork,
        ILogger<SmtpConfigurationResolver> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<EmailConfiguration>> GetSmtpHierarchyAsync(
        Guid? tenantId = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var hierarchy = new List<EmailConfiguration>();

        _logger.LogDebug("Resolving SMTP hierarchy for TenantId: {TenantId}, UserId: {UserId}", tenantId, userId);

        // 1. User-level active SMTP (highest priority)
        if (userId.HasValue)
        {
            var userSmtp = await _unitOfWork.Repository<EmailConfiguration>()
                .GetQueryable()
                .Where(c =>
                    c.UserId == userId &&
                    c.IsActive &&
                    !c.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (userSmtp != null)
            {
                hierarchy.Add(userSmtp);
                _logger.LogDebug("Found user-level SMTP configuration: {FromEmail}", userSmtp.FromEmail);
            }
        }

        // 2. Tenant-level active SMTP (medium priority)
        if (tenantId.HasValue)
        {
            var tenantSmtp = await _unitOfWork.Repository<EmailConfiguration>()
                .GetQueryable()
                .Where(c =>
                    c.TenantId == tenantId &&
                    c.UserId == null &&
                    c.IsActive &&
                    !c.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (tenantSmtp != null)
            {
                hierarchy.Add(tenantSmtp);
                _logger.LogDebug("Found tenant-level SMTP configuration: {FromEmail}", tenantSmtp.FromEmail);
            }
        }

        // 3. System-level SMTP (always fallback, lowest priority)
        var systemSmtp = await _unitOfWork.Repository<EmailConfiguration>()
            .GetQueryable()
            .Where(c =>
                c.IsSystemDefault &&
                c.IsActive &&
                !c.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (systemSmtp != null)
        {
            hierarchy.Add(systemSmtp);
            _logger.LogDebug("Found system-level SMTP configuration: {FromEmail}", systemSmtp.FromEmail);
        }

        if (!hierarchy.Any())
        {
            _logger.LogWarning("No SMTP configuration found at any level for TenantId: {TenantId}, UserId: {UserId}",
                tenantId, userId);
        }
        else
        {
            _logger.LogInformation("SMTP hierarchy resolved with {Count} configuration(s)", hierarchy.Count);
        }

        return hierarchy;
    }

    public async Task<EmailConfiguration?> GetActiveSmtpConfigurationAsync(
        Guid? tenantId = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var hierarchy = await GetSmtpHierarchyAsync(tenantId, userId, cancellationToken);
        return hierarchy.FirstOrDefault();
    }
}
