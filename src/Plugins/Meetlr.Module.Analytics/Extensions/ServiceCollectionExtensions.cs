using FluentValidation;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Module.Analytics.Endpoints.Public.TrackPageView;
using Meetlr.Module.Analytics.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Meetlr.Module.Analytics.Extensions;

/// <summary>
/// Extension methods for registering Analytics plugin services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add the Analytics plugin to the service collection and register its controllers
    /// </summary>
    public static IMvcBuilder AddAnalyticsPlugin(this IMvcBuilder mvcBuilder)
    {
        var services = mvcBuilder.Services;

        // Register plugin's DB configuration so its entities are included in migrations
        services.AddSingleton<IPluginDbConfiguration, AnalyticsPluginDbConfiguration>();

        // Add MediatR handlers from this assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

        // Add FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

        // Register plugin controllers
        mvcBuilder.AddApplicationPart(typeof(TrackPageViewEndpoint).Assembly);

        return mvcBuilder;
    }
}
