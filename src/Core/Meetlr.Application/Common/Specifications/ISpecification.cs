using System.Linq.Expressions;

namespace Meetlr.Application.Common.Specifications;

/// <summary>
/// Specification pattern interface for building reusable query logic
/// </summary>
public interface ISpecification<TEntity>
{
    /// <summary>
    /// Main filter criteria (WHERE clause)
    /// </summary>
    Expression<Func<TEntity, bool>>? Criteria { get; }

    /// <summary>
    /// Include expressions for eager loading
    /// </summary>
    List<Expression<Func<TEntity, object>>> Includes { get; }

    /// <summary>
    /// String-based includes for nested navigation properties (e.g., "User.StripeAccount")
    /// </summary>
    List<string> IncludeStrings { get; }

    /// <summary>
    /// Order by expression (ascending)
    /// </summary>
    Expression<Func<TEntity, object>>? OrderBy { get; }

    /// <summary>
    /// Order by expression (descending)
    /// </summary>
    Expression<Func<TEntity, object>>? OrderByDescending { get; }

    /// <summary>
    /// Number of records to skip (for pagination)
    /// </summary>
    int Skip { get; }

    /// <summary>
    /// Number of records to take (for pagination)
    /// </summary>
    int Take { get; }

    /// <summary>
    /// Whether pagination is enabled
    /// </summary>
    bool IsPagingEnabled { get; }
}
