using Meetlr.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Meetlr.Infrastructure.Data.Seeding;

/// <summary>
/// Orchestrator service for executing test data seeders
/// </summary>
public class TestDataSeederService
{
    private readonly IEnumerable<ISeeder> _seeders;
    private readonly ILogger<TestDataSeederService> _logger;

    public TestDataSeederService(
        IEnumerable<ISeeder> seeders,
        ILogger<TestDataSeederService> logger)
    {
        _seeders = seeders;
        _logger = logger;
    }

    public async Task SeedAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting test data seeding process...");

        try
        {
            // Order seeders by their Order property
            var orderedSeeders = _seeders.OrderBy(s => s.Order).ToList();

            _logger.LogInformation("Found {Count} seeders to execute", orderedSeeders.Count);

            foreach (var seeder in orderedSeeders)
            {
                var seederName = seeder.GetType().Name;

                try
                {
                    _logger.LogInformation("Executing seeder: {SeederName} (Order: {Order})",
                        seederName, seeder.Order);

                    await seeder.SeedAsync(cancellationToken);

                    _logger.LogInformation("Successfully completed seeder: {SeederName}", seederName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while executing seeder: {SeederName}", seederName);
                    throw;
                }
            }

            _logger.LogInformation("Test data seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test data seeding process failed");
            throw;
        }
    }
}
