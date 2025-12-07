using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Module.Calendar.Application.Interfaces;
using Meetlr.Module.Calendar.Infrastructure;
using Meetlr.Module.Calendar.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Meetlr.Module.Calendar;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the Calendar module services and controllers
    /// </summary>
    public static IMvcBuilder AddCalendarModule(
        this IMvcBuilder mvcBuilder,
        IConfiguration configuration)
    {
        var services = mvcBuilder.Services;

        // Register plugin DB configuration for entity discovery
        services.AddScoped<IPluginDbConfiguration, CalendarPluginDbConfiguration>();

        // Register calendar provider services (OAuth + calendar operations)
        services.AddScoped<ICalendarProviderService, GoogleCalendarProviderService>();
        services.AddScoped<ICalendarProviderService, MicrosoftCalendarProviderService>();

        // Register calendar invite generator (ICS file generation)
        services.AddScoped<ICalendarInviteGenerator, CalendarInviteGenerator>();

        // Register ICalendarService - aggregates calendar operations across all providers
        // This is the public interface exposed to the rest of the application
        services.AddScoped<ICalendarService, CalendarService>();

        // Register MediatR handlers from this assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // Register plugin controllers
        mvcBuilder.AddApplicationPart(typeof(DependencyInjection).Assembly);

        return mvcBuilder;
    }
}
