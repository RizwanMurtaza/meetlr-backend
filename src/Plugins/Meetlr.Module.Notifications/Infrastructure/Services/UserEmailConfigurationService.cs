using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Plugins.Services;
using Meetlr.Domain.Entities.Emails;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Module.Notifications.Infrastructure.Services;

/// <summary>
/// Service for creating default email configurations for new users.
/// Seeds Oracle Email Delivery Service SMTP configuration at user level.
/// </summary>
public class UserEmailConfigurationService : IUserEmailConfigurationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserEmailConfigurationService> _logger;

    public UserEmailConfigurationService(
        IUnitOfWork unitOfWork,
        ILogger<UserEmailConfigurationService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task CreateDefaultEmailConfigurationAsync(
        Guid userId,
        Guid tenantId,
        string userEmail,
        string userName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Creating default email configuration for user {UserId} in tenant {TenantId}",
            userId, tenantId);

        try
        {
            // Check if user already has an email configuration
            var existingConfig = await _unitOfWork.Repository<EmailConfiguration>()
                .GetQueryable()
                .FirstOrDefaultAsync(
                    c => c.UserId == userId && c.TenantId == tenantId,
                    cancellationToken);

            if (existingConfig != null)
            {
                _logger.LogInformation(
                    "User {UserId} already has an email configuration, skipping creation",
                    userId);
                return;
            }

            // Create user-level Oracle Email Delivery Service configuration
            var emailConfig = new EmailConfiguration
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = userId,
                SmtpHost = "smtp.eu-frankfurt-1.oraclecloud.com",
                SmtpPort = 587,
                SmtpUsername = "ocid1.user.oc1..aaaaaaaa2tfhgx6hgz5rq6rw5jdxvqdll45mnjm6l5t2udlnqlv3ihn3ua5q@ocid1.tenancy.oc1..aaaaaaaassaidldj7jrxtq3fqtm4spi2rop3czmqxd5yq3ycebeojt4f63ea.2e.com",
                SmtpPassword = "<[NH&QoAf(ZoE_wz4n4$",
                FromEmail = userEmail,
                FromName = userName,
                EnableSsl = true,
                IsActive = true,
                IsSystemDefault = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId.ToString()
            };

            _unitOfWork.Repository<EmailConfiguration>().Add(emailConfig);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Created default email configuration {ConfigId} for user {UserId}",
                emailConfig.Id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create default email configuration for user {UserId}",
                userId);
            // Don't throw - email config creation failure shouldn't block registration
        }
    }
}
