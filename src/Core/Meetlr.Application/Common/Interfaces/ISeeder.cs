namespace Meetlr.Application.Common.Interfaces;

/// <summary>
/// Interface for database seeders
/// Seeders run in order based on the Order property
/// </summary>
public interface ISeeder
{
    /// <summary>
    /// Seeds data into the database
    /// </summary>
    Task SeedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Order in which this seeder should run (lower values run first)
    /// </summary>
    int Order { get; }
}
