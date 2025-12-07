using Meetlr.Domain.Entities.Emails;
using Meetlr.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Meetlr.Module.Notifications.Infrastructure.Data.Seeding;

/// <summary>
/// Seeds system-level email provider configurations (SendGrid, Mailchimp)
/// These can be updated by admins through the UI
/// </summary>
public class EmailProviderConfigurationSeeder
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailProviderConfigurationSeeder> _logger;

    public EmailProviderConfigurationSeeder(
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<EmailProviderConfigurationSeeder> logger)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting email provider configuration seeding...");

        // Check if any email provider configurations exist
        var existingConfigs = await _unitOfWork.Repository<EmailProviderConfiguration>()
            .GetQueryable()
            .AnyAsync(cancellationToken);

        if (existingConfigs)
        {
            _logger.LogInformation("Email provider configurations already exist, skipping seeding");
            return;
        }

        var configurations = new List<EmailProviderConfiguration>();

        // SendGrid configuration from appsettings
        var sendGridApiKey = _configuration["SendGrid:ApiKey"];
        var sendGridFromEmail = _configuration["SendGrid:FromEmail"] ?? "noreply@meetlr.com";
        var sendGridFromName = _configuration["SendGrid:FromName"] ?? "Meetlr";

        if (!string.IsNullOrEmpty(sendGridApiKey))
        {
            configurations.Add(new EmailProviderConfiguration
            {
                Id = Guid.NewGuid(),
                ProviderType = "SendGrid",
                ApiKey = sendGridApiKey,
                DefaultFromEmail = sendGridFromEmail,
                DefaultFromName = sendGridFromName,
                IsActive = true,
                IsSystemDefault = true,
                TenantId = null,
                UserId = null,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
            _logger.LogInformation("SendGrid configuration loaded from appsettings");
        }
        else
        {
            _logger.LogWarning("SendGrid API key not found in appsettings. Skipping SendGrid seeding.");
        }

        // Mailchimp/Mandrill configuration from appsettings
        var mailchimpApiKey = _configuration["Mailchimp:ApiKey"];
        var mailchimpFromEmail = _configuration["Mailchimp:FromEmail"] ?? "noreply@meetlr.com";
        var mailchimpFromName = _configuration["Mailchimp:FromName"] ?? "Meetlr";

        if (!string.IsNullOrEmpty(mailchimpApiKey))
        {
            configurations.Add(new EmailProviderConfiguration
            {
                Id = Guid.NewGuid(),
                ProviderType = "Mailchimp",
                ApiKey = mailchimpApiKey,
                DefaultFromEmail = mailchimpFromEmail,
                DefaultFromName = mailchimpFromName,
                IsActive = true,
                IsSystemDefault = true,
                TenantId = null,
                UserId = null,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
            _logger.LogInformation("Mailchimp configuration loaded from appsettings");
        }

        if (configurations.Count == 0)
        {
            _logger.LogWarning("No email provider configurations found in appsettings. Skipping seeding.");
            return;
        }

        foreach (var config in configurations)
        {
            _unitOfWork.Repository<EmailProviderConfiguration>().Add(config);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeded {Count} email provider configurations", configurations.Count);
    }
}
