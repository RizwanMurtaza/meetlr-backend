using FluentValidation;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Module.SlotInvitation.Infrastructure;
using Meetlr.Module.SlotInvitation.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Meetlr.Module.SlotInvitation;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the SlotInvitation module services and controllers
    /// </summary>
    public static IMvcBuilder AddSlotInvitationModule(
        this IMvcBuilder mvcBuilder,
        IConfiguration configuration)
    {
        var services = mvcBuilder.Services;

        // Register plugin DB configuration for entity discovery
        services.AddScoped<IPluginDbConfiguration, SlotInvitationPluginDbConfiguration>();

        // Register the SlotInvitationService - this exposes slot reservations to the core availability handlers
        services.AddScoped<ISlotInvitationService, SlotInvitationService>();

        // Register MediatR handlers from this assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // Register FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Register plugin controllers
        mvcBuilder.AddApplicationPart(typeof(DependencyInjection).Assembly);

        return mvcBuilder;
    }
}
