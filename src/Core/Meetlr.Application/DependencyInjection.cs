using System.Reflection;
using FluentValidation;
using MediatR;
using Meetlr.Application.Common.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace Meetlr.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Register FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register MediatR pipeline behaviors (order matters!)
        // 1. Validation runs first for all requests
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        // 2. Automatic query caching (checks cache before executing query)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AutomaticQueryCachingBehavior<,>));

        // Note: RecurrenceOccurrenceGenerator removed - no longer needed with simplified series model
        // Series now use explicitly selected date times instead of generating from recurrence patterns

        // 100% Pure CQRS Architecture - No service layer in Application
        // All business logic is handled through Commands and Queries via MediatR
        // Handlers compose granular operations by calling other queries/commands

        return services;
    }
}
