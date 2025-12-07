using System.Collections.Concurrent;
using System.Text.Json;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Common.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Meetlr.Infrastructure.Services.Caching;

/// <summary>
/// In-memory cache implementation using IMemoryCache
/// Suitable for development and single-server deployments
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly CacheSettings _settings;
    private readonly ConcurrentDictionary<string, byte> _keyTracker;

    public MemoryCacheService(IMemoryCache cache, IOptions<CacheSettings> settings)
    {
        _cache = cache;
        _settings = settings.Value;
        _keyTracker = new ConcurrentDictionary<string, byte>();
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        var fullKey = GetFullKey(key);

        if (_cache.TryGetValue(fullKey, out string? cachedJson) && cachedJson != null)
        {
            try
            {
                var value = JsonSerializer.Deserialize<T>(cachedJson);
                return Task.FromResult(value);
            }
            catch (JsonException)
            {
                // If deserialization fails, remove the corrupted entry
                _cache.Remove(fullKey);
                _keyTracker.TryRemove(fullKey, out _);
            }
        }

        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var fullKey = GetFullKey(key);
        var duration = expiration ?? _settings.GetDefaultDuration();

        var json = JsonSerializer.Serialize(value);

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = duration,
            Size = 1 // For memory cache size limit
        };

        // Add callback to remove from key tracker when expired
        cacheOptions.RegisterPostEvictionCallback((evictedKey, evictedValue, reason, state) =>
        {
            if (evictedKey is string keyString)
            {
                _keyTracker.TryRemove(keyString, out _);
            }
        });

        _cache.Set(fullKey, json, cacheOptions);
        _keyTracker.TryAdd(fullKey, 0);

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        _cache.Remove(fullKey);
        _keyTracker.TryRemove(fullKey, out _);

        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var fullPrefix = GetFullKey(prefix);

        // Find all keys matching the prefix
        var keysToRemove = _keyTracker.Keys
            .Where(k => k.StartsWith(fullPrefix, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            _keyTracker.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        var exists = _cache.TryGetValue(fullKey, out _);
        return Task.FromResult(exists);
    }

    private string GetFullKey(string key)
    {
        return string.IsNullOrEmpty(_settings.KeyPrefix)
            ? key
            : $"{_settings.KeyPrefix}:{key}";
    }
}
