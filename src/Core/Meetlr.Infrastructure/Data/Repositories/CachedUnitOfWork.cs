using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Meetlr.Infrastructure.Data.Repositories;

/// <summary>
/// Unit of Work decorator that automatically invalidates user-specific cache on SaveChanges
/// This ensures cache is cleared whenever any data is modified for a user
/// NO CODE CHANGES REQUIRED - caching is completely transparent
/// </summary>
public class CachedUnitOfWork : IUnitOfWork
{
    private readonly IUnitOfWork _innerUnitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CachedUnitOfWork> _logger;

    public CachedUnitOfWork(
        IUnitOfWork innerUnitOfWork,
        ICacheService cacheService,
        ICurrentUserService currentUserService,
        ILogger<CachedUnitOfWork> logger)
    {
        _innerUnitOfWork = innerUnitOfWork;
        _cacheService = cacheService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public IRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        // Simply delegate to inner UnitOfWork
        // We don't wrap repositories because queries use GetQueryable() which can't be cached
        return _innerUnitOfWork.Repository<TEntity>();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Save changes first
        var result = await _innerUnitOfWork.SaveChangesAsync(cancellationToken);

        // If changes were saved and we have a user context, invalidate all caches for this user
        if (result > 0 && _currentUserService.UserId.HasValue)
        {
            var userId = _currentUserService.UserId.Value;
            var cachePrefix = $"user:{userId}";

            _logger.LogInformation(
                "Cache invalidation: Clearing all caches for user {UserId} after {Count} entities modified",
                userId,
                result);

            await _cacheService.RemoveByPrefixAsync(cachePrefix, cancellationToken);
        }

        return result;
    }

    public void Dispose()
    {
        _innerUnitOfWork.Dispose();
    }
}
