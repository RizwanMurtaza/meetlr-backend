using System.Text.Json;
using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Common.Settings;
using Meetlr.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Meetlr.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that automatically caches ALL query results
/// - Serializes the entire request object as cache key
/// - Only caches queries (IRequest with non-void response)
/// - Skips commands (IRequest with void/int/bool response)
/// - 100% automatic - no code changes needed in queries
/// </summary>
public class AutomaticQueryCachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
    where TResponse : class
{
    private readonly ICacheService _cacheService;
    private readonly ICurrentUserService _currentUserService;
    private readonly CacheSettings _cacheSettings;
    private readonly ILogger<AutomaticQueryCachingBehavior<TRequest, TResponse>> _logger;

    public AutomaticQueryCachingBehavior(
        ICacheService cacheService,
        ICurrentUserService currentUserService,
        IOptions<CacheSettings> cacheSettings,
        ILogger<AutomaticQueryCachingBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _currentUserService = currentUserService;
        _cacheSettings = cacheSettings.Value;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only cache if enabled and user is logged in
        if (!_cacheSettings.Enabled || !_currentUserService.UserId.HasValue)
        {
            return await next();
        }

        // Only cache queries (non-void responses that look like queries)
        if (!IsQuery<TResponse>())
        {
            return await next();
        }

        // Generate cache key from the entire request object
        var cacheKey = GenerateCacheKey(request, _currentUserService.UserId.Value);

        _logger.LogDebug("Query cache key: {CacheKey} for {RequestType}",
            cacheKey, typeof(TRequest).Name);

        // Try to get from cache
        var cachedResponse = await _cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);
        if (cachedResponse != null)
        {
            _logger.LogInformation("✅ Cache HIT for {RequestType}", typeof(TRequest).Name);
            return cachedResponse;
        }

        _logger.LogInformation("❌ Cache MISS for {RequestType} - executing query", typeof(TRequest).Name);

        // Execute the query
        var response = await next();

        // Cache the result
        if (response != null)
        {
            var cacheDuration = _cacheSettings.GetDefaultDuration();
            await _cacheService.SetAsync(cacheKey, response, cacheDuration, cancellationToken);

            _logger.LogDebug("Cached {RequestType} result for {Duration}",
                typeof(TRequest).Name, cacheDuration);
        }

        return response!;
    }

    /// <summary>
    /// Determines if this is a query (cacheable) or command (not cacheable)
    /// Commands typically return void, int, bool, or have "Command" in the name
    /// Queries return DTOs, lists, or have "Query" in the name
    /// </summary>
    private static bool IsQuery<T>()
    {
        var responseType = typeof(T);
        var requestType = typeof(TRequest);

        // If request name contains "Command", it's a command - don't cache
        if (requestType.Name.Contains("Command", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // If response is void, int, bool - it's likely a command - don't cache
        if (responseType == typeof(void) ||
            responseType == typeof(Task) ||
            responseType == typeof(int) ||
            responseType == typeof(bool) ||
            responseType == typeof(Unit))
        {
            return false;
        }

        // Everything else is considered a query
        return true;
    }

    /// <summary>
    /// Generates cache key from request object
    /// Format: user:{userId}:{RequestType}:{hash-of-serialized-request}
    /// </summary>
    private static string GenerateCacheKey(TRequest request, Guid userId)
    {
        var requestType = typeof(TRequest).Name;

        // Serialize the entire request to JSON
        var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Generate hash from JSON
        var hash = GenerateHash(requestJson);

        // Format: user:{userId}:{RequestType}:{hash}
        return $"user:{userId}:{requestType}:{hash}";
    }

    private static string GenerateHash(string input)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hashBytes)[..12]; // Take first 12 characters
    }
}
