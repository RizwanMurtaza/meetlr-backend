using Meetlr.Application.Common.Interfaces;
using Meetlr.Module.Board.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Meetlr.Module.Board;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the Board module services and controllers
    /// </summary>
    public static IMvcBuilder AddBoardModule(
        this IMvcBuilder mvcBuilder,
        IConfiguration configuration)
    {
        var services = mvcBuilder.Services;

        // Register plugin DB configuration for entity discovery
        services.AddScoped<IPluginDbConfiguration, BoardPluginDbConfiguration>();

        // Register MediatR handlers from this assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // Register plugin controllers
        mvcBuilder.AddApplicationPart(typeof(DependencyInjection).Assembly);

        return mvcBuilder;
    }
}
