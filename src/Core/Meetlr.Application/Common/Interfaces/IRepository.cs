using Meetlr.Application.Common.Specifications;

namespace Meetlr.Application.Common.Interfaces;

/// <summary>
/// Generic repository interface for data access
/// Provides abstraction over data access layer
/// </summary>
public interface IRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Get entity by ID
    /// </summary>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Find entities matching a specification
    /// </summary>
    Task<IReadOnlyList<TEntity>> FindAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get first entity matching a specification or null
    /// </summary>
    Task<TEntity?> FirstOrDefaultAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Count entities matching a specification
    /// </summary>
    Task<int> CountAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if any entities match a specification
    /// </summary>
    Task<bool> AnyAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get queryable for advanced scenarios (use sparingly)
    /// </summary>
    IQueryable<TEntity> GetQueryable();

    /// <summary>
    /// Add entity to repository
    /// </summary>
    void Add(TEntity entity);

    /// <summary>
    /// Add multiple entities to repository (bulk insert)
    /// </summary>
    void AddRange(IEnumerable<TEntity> entities);

    /// <summary>
    /// Update entity in repository
    /// </summary>
    void Update(TEntity entity);

    /// <summary>
    /// Delete entity from repository
    /// </summary>
    void Delete(TEntity entity);
}
