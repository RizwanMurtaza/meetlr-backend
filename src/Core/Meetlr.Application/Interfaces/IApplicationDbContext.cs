namespace Meetlr.Application.Interfaces;

/// <summary>
/// Application database context interface
/// Note: This interface is kept minimal. Use IUnitOfWork for data access operations.
/// </summary>
public interface IApplicationDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
