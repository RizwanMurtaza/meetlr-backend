using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Common.Settings;
using Meetlr.Application.Interfaces;
using Meetlr.Application.Plugins;
using Meetlr.Application.Plugins.Payments;
using Meetlr.Infrastructure.Data;
using Meetlr.Infrastructure.Data.Repositories;
using Meetlr.Infrastructure.Data.Seeding;
using Meetlr.Infrastructure.Plugins.Payments;
using Meetlr.Infrastructure.Services;
using Meetlr.Infrastructure.Services.Caching;
using Meetlr.Infrastructure.Services.FileStorage;
using Meetlr.Infrastructure.Services.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Meetlr.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                b => b.MigrationsAssembly("Meetlr.Infrastructure")
                      .MigrationsHistoryTable("__EFMigrationsHistory")));

        // Domain Event Dispatcher
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // Unit of Work and Repository Pattern with automatic caching
        // Register base UnitOfWork first, then decorate it with caching
        services.AddScoped<UnitOfWork>();
        services.AddScoped<IUnitOfWork>(provider =>
        {
            var context = provider.GetRequiredService<ApplicationDbContext>();
            var cacheService = provider.GetRequiredService<ICacheService>();
            var currentUserService = provider.GetRequiredService<ICurrentUserService>();
            var logger = provider.GetRequiredService<ILogger<CachedUnitOfWork>>();
            var domainEventDispatcher = provider.GetRequiredService<IDomainEventDispatcher>();

            var baseUnitOfWork = new UnitOfWork(context, domainEventDispatcher);
            return new CachedUnitOfWork(baseUnitOfWork, cacheService, currentUserService, logger);
        });

        // Keep IApplicationDbContext for backward compatibility (will be phased out)
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // HttpClient
        services.AddHttpClient();

        // Services
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<ITimeZoneService, TimeZoneService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IOAuthService, OAuthService>();
        services.AddScoped<JwtTokenService>();
        services.AddScoped<IOtpService, OtpService>();

        // Note: Email services (IEmailService, IEmailTemplateRenderer, ISmtpConfigurationResolver,
        // IEmailProviderConfigurationResolver, IEmailProvider) are registered in Meetlr.Module.Notifications

        // OAuth Services - shared by Calendar and MeetingTypes plugins
        services.AddScoped<GoogleOAuthService>();
        services.AddScoped<MicrosoftOAuthService>();

        // Plugin System
        // Unified Plugin Factory - collects all IPlugin implementations from DI
        services.AddScoped<IPluginFactory, PluginFactory>();

        // Payment Provider Factory (Legacy - will be replaced by IPluginFactory) - also SCOPED to match plugins
        services.AddScoped<IPaymentProviderFactory, PaymentProviderFactory>();

        // Notification Services
        services.AddScoped<ISmsNotificationService, SmsNotificationService>();
        services.AddScoped<IWhatsAppNotificationService, WhatsAppNotificationService>();
        services.AddScoped<INotificationQueueService, NotificationQueueService>();

        // Notification Sub-Services (Refactored for better organization)
        services.AddScoped<NotificationChannelDispatcher>();
        services.AddScoped<SingleBookingNotificationService>();
        services.AddScoped<RecurringBookingsNotificationService>();

        // File Storage
        services.Configure<FileStorageSettings>(configuration.GetSection("FileStorage"));
        services.AddScoped<IFileStorageService, HybridFileStorageService>();

        // Caching
        services.AddMemoryCache(options =>
        {
            var cacheSettings = configuration.GetSection("Cache").Get<CacheSettings>();
            if (cacheSettings != null)
            {
                options.SizeLimit = cacheSettings.MemoryCacheMaxEntries;
            }
        });
        services.AddSingleton<ICacheService, MemoryCacheService>();

        // Note: Background Services moved to Meetlr.Scheduler project
        // Call services.AddSchedulerServices() in Program.cs

        // Database Seeders
        // Note: Email seeders (SystemEmailConfigurationSeeder, EmailTemplateSeeder, EmailProviderConfigurationSeeder)
        // are now registered in Meetlr.Module.Notifications module
        services.AddScoped<SystemDataSeeder>();
        services.AddScoped<TestDataSeederService>();

        return services;
    }
}
