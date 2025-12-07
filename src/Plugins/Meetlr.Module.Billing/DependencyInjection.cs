using FluentValidation;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Plugins.Services;
using Meetlr.Module.Billing.Application.Interfaces;
using Meetlr.Module.Billing.Infrastructure;
using Meetlr.Module.Billing.Infrastructure.Seeding;
using Meetlr.Module.Billing.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Meetlr.Module.Billing;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the Billing module services and controllers
    /// </summary>
    public static IMvcBuilder AddBillingModule(
        this IMvcBuilder mvcBuilder,
        IConfiguration configuration)
    {
        var services = mvcBuilder.Services;

        // Register plugin DB configuration for entity discovery
        services.AddScoped<IPluginDbConfiguration, BillingPluginDbConfiguration>();

        // Register the CreditService
        services.AddScoped<ICreditService, CreditService>();

        // Register the UserBillingService (used by registration handlers to assign free package)
        services.AddScoped<IUserBillingService, UserBillingService>();

        // Register the NotificationBillingService (used by notification handlers to check/deduct credits)
        services.AddScoped<INotificationBillingService, NotificationBillingService>();

        // Register the seeder for billing data (packages, credit costs)
        services.AddScoped<ISeeder, BillingSeeder>();

        // Register MediatR handlers from this assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // Register FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Register plugin controllers
        mvcBuilder.AddApplicationPart(typeof(DependencyInjection).Assembly);

        return mvcBuilder;
    }
}
