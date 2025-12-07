using FluentValidation;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Application.Plugins.Services;
using Meetlr.Module.Notifications.Infrastructure;
using Meetlr.Module.Notifications.Infrastructure.Data.Seeding;
using Meetlr.Module.Notifications.Infrastructure.Services;
using Meetlr.Module.Notifications.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Meetlr.Module.Notifications;

/// <summary>
/// Dependency injection configuration for notifications module
/// Provides email, SMS, and WhatsApp notification services with multiple provider support
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddNotificationsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register plugin DB configuration for entity discovery
        services.AddScoped<IPluginDbConfiguration, NotificationsPluginDbConfiguration>();

        // Register MediatR handlers from this assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // Register FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Register HTTP client for Mailchimp/Mandrill API
        services.AddHttpClient("Mailchimp");

        // Register core email services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IEmailTemplateRenderer, EmailTemplateRenderer>();
        services.AddScoped<ISmtpConfigurationResolver, SmtpConfigurationResolver>();
        services.AddScoped<IEmailProviderConfigurationResolver, EmailProviderConfigurationResolver>();

        // Register email providers (configuration comes from database)
        // Priority order: SendGrid (10) → Mailchimp (50) → SMTP (100)
        services.AddScoped<IEmailProvider, SendGridEmailProvider>();
        services.AddScoped<IEmailProvider, MailchimpEmailProvider>();
        services.AddScoped<IEmailProvider, SmtpEmailProvider>();

        // Register user email configuration service (used during registration to seed user-level config)
        services.AddScoped<IUserEmailConfigurationService, UserEmailConfigurationService>();

        // Register event email template seeder (used when creating new events)
        services.AddScoped<IEventEmailTemplateSeeder, EventEmailTemplateSeeder>();

        // Register seeders (individual seeders used by the combined EmailSeeder)
        services.AddScoped<SystemEmailConfigurationSeeder>();
        services.AddScoped<EmailTemplateSeeder>();
        services.AddScoped<EmailProviderConfigurationSeeder>();

        // Register the combined seeder that implements ISeeder
        services.AddScoped<ISeeder, EmailSeeder>();

        return services;
    }
}
