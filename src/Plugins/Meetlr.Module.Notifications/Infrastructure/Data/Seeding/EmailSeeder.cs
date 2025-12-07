using Meetlr.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Meetlr.Module.Notifications.Infrastructure.Data.Seeding;

/// <summary>
/// Combined seeder for all email-related data
/// Orchestrates seeding of system email configuration, templates, and provider configurations
/// </summary>
public class EmailSeeder : ISeeder
{
    private readonly SystemEmailConfigurationSeeder _systemEmailConfigurationSeeder;
    private readonly EmailTemplateSeeder _emailTemplateSeeder;
    private readonly EmailProviderConfigurationSeeder _emailProviderConfigurationSeeder;
    private readonly ILogger<EmailSeeder> _logger;

    public EmailSeeder(
        SystemEmailConfigurationSeeder systemEmailConfigurationSeeder,
        EmailTemplateSeeder emailTemplateSeeder,
        EmailProviderConfigurationSeeder emailProviderConfigurationSeeder,
        ILogger<EmailSeeder> logger)
    {
        _systemEmailConfigurationSeeder = systemEmailConfigurationSeeder;
        _emailTemplateSeeder = emailTemplateSeeder;
        _emailProviderConfigurationSeeder = emailProviderConfigurationSeeder;
        _logger = logger;
    }

    /// <summary>
    /// Email seeders should run early to ensure email configuration is available
    /// </summary>
    public int Order => 10;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting email module seeding...");

        // Seed system email (SMTP) configuration first
        await _systemEmailConfigurationSeeder.SeedAsync(cancellationToken);

        // Seed email templates
        await _emailTemplateSeeder.SeedAsync(cancellationToken);

        // Seed email provider configurations (SendGrid, Mailchimp)
        await _emailProviderConfigurationSeeder.SeedAsync(cancellationToken);

        _logger.LogInformation("Email module seeding completed");
    }
}
