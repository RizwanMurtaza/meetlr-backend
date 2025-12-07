using Meetlr.Application.Common.Interfaces;
using Meetlr.Infrastructure.Data.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Meetlr.Infrastructure.Data;

/// <summary>
/// Initializes the database by applying migrations and seeding data
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Initializes the database with migrations and seed data
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            // Get database context
            var context = services.GetRequiredService<ApplicationDbContext>();

            // Get environment
            var environment = services.GetRequiredService<IHostEnvironment>();

            logger.LogInformation("Starting database initialization...");

            // Apply any pending migrations
            logger.LogInformation("Applying database migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully");

            // Seed system data (always runs in all environments)
            logger.LogInformation("Seeding system data...");
            var systemSeeder = services.GetRequiredService<SystemDataSeeder>();
            await systemSeeder.SeedAsync();
            logger.LogInformation("System data seeded successfully");

            // Run all registered ISeeder implementations (includes plugin/module seeders)
            // Note: Email module (Meetlr.Module.Notifications) provides EmailSeeder which handles
            // SystemEmailConfiguration, EmailTemplate, and EmailProviderConfiguration seeding
            logger.LogInformation("Running registered seeders...");
            var seeders = services.GetServices<ISeeder>().OrderBy(s => s.Order);
            foreach (var seeder in seeders)
            {
                logger.LogInformation("Running seeder: {SeederType}", seeder.GetType().Name);
                await seeder.SeedAsync();
            }
            logger.LogInformation("All seeders completed successfully");

            // Seed test data (only in Development environment)
            if (environment.IsDevelopment())
            {
                logger.LogInformation("Development environment detected. Seeding test data...");
                // var testDataSeeder = services.GetRequiredService<TestDataSeederService>();
                // await testDataSeeder.SeedAllAsync();
                logger.LogInformation("Test data seeded successfully");
            }
            else
            {
                logger.LogInformation("Non-development environment. Skipping test data seeding.");
            }

            logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database");
            throw;
        }
    }
}
