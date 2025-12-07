using Meetlr.Domain.Entities.Emails;
using Meetlr.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Meetlr.Module.Notifications.Infrastructure.Data.Seeding;

/// <summary>
/// Seeds the system-level email configuration (used as fallback)
/// </summary>
public class SystemEmailConfigurationSeeder
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SystemEmailConfigurationSeeder> _logger;

    public SystemEmailConfigurationSeeder(
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<SystemEmailConfigurationSeeder> logger)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting system email configuration seeding...");

        try
        {
            // Check if system email configuration already exists
            var existingConfig = await _unitOfWork.Repository<EmailConfiguration>()
                .GetQueryable()
                .FirstOrDefaultAsync(
                    e => e.TenantId == null && e.UserId == null && e.IsSystemDefault,
                    cancellationToken);

            if (existingConfig != null)
            {
                _logger.LogInformation("System email configuration already exists, skipping seeding");
                return;
            }

            // Read email configuration from appsettings
            var smtpHost = _configuration["SystemEmail:SmtpHost"];
            var smtpUsername = _configuration["SystemEmail:SmtpUsername"];
            var smtpPassword = _configuration["SystemEmail:SmtpPassword"];

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
            {
                _logger.LogWarning("System email configuration not found in appsettings. Skipping seeding.");
                return;
            }

            var systemConfig = new EmailConfiguration
            {
                Id = Guid.NewGuid(),
                TenantId = null,  // System level
                UserId = null,    // System level
                SmtpHost = smtpHost,
                SmtpPort = _configuration.GetValue<int>("SystemEmail:SmtpPort", 587),
                SmtpUsername = smtpUsername,
                SmtpPassword = smtpPassword,
                FromEmail = _configuration["SystemEmail:FromEmail"] ?? "noreply@meetlr.com",
                FromName = _configuration["SystemEmail:FromName"] ?? "Meetlr",
                EnableSsl = _configuration.GetValue<bool>("SystemEmail:EnableSsl", true),
                IsActive = true,
                IsSystemDefault = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };

            _unitOfWork.Repository<EmailConfiguration>().Add(systemConfig);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("System email configuration created with Oracle Email Delivery Service");

            _logger.LogInformation("System email configuration seeded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding system email configuration");
            throw;
        }
    }
}
