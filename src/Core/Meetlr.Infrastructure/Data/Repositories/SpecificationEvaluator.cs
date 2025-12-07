using Meetlr.Application.Common.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Infrastructure.Data.Repositories;

/// <summary>
/// Evaluates specifications and applies them to IQueryable
/// </summary>
public static class SpecificationEvaluator
{
    /// <summary>
    /// Applies a specification to a queryable
    /// </summary>
    public static IQueryable<TEntity> GetQuery<TEntity>(
        IQueryable<TEntity> inputQuery,
        ISpecification<TEntity> specification) where TEntity : class
    {
        var query = inputQuery;

        // Apply AsNoTracking for read-only queries (performance optimization)
        // Specifications are typically used for queries, not updates
        query = query.AsNoTracking();

        // Apply filter criteria
        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        // Apply includes
        query = specification.Includes
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply string-based includes (for nested navigation properties)
        query = specification.IncludeStrings
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply ordering
        if (specification.OrderBy != null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending != null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        // Apply paging
        if (specification.IsPagingEnabled)
        {
            query = query.Skip(specification.Skip).Take(specification.Take);
        }

        return query;
    }
}
