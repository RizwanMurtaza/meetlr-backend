using Meetlr.Application.Common.Interfaces;
using Meetlr.Module.Homepage.Infrastructure;
using Meetlr.Module.Homepage.Infrastructure.Seeding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Meetlr.Module.Homepage;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the Homepage module services and controllers
    /// </summary>
    public static IMvcBuilder AddHomepageModule(
        this IMvcBuilder mvcBuilder,
        IConfiguration configuration)
    {
        var services = mvcBuilder.Services;

        // Register plugin DB configuration for entity discovery
        services.AddScoped<IPluginDbConfiguration, HomepagePluginDbConfiguration>();

        // Register MediatR handlers from this assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // Register seeders
        services.AddScoped<ISeeder, HomepageTemplateSeeder>();

        // Register plugin controllers
        mvcBuilder.AddApplicationPart(typeof(DependencyInjection).Assembly);

        return mvcBuilder;
    }
}
