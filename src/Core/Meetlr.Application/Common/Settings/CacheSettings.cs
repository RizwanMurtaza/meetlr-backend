namespace Meetlr.Application.Common.Settings;

public class CacheSettings
{
    /// <summary>
    /// Whether caching is enabled globally
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Default cache duration in seconds
    /// </summary>
    public int DefaultDurationSeconds { get; set; } = 300; // 5 minutes

    /// <summary>
    /// Cache provider: "Memory" or "Redis"
    /// </summary>
    public string Provider { get; set; } = "Memory";

    /// <summary>
    /// Redis connection string (only used if Provider is "Redis")
    /// </summary>
    public string? RedisConnectionString { get; set; }

    /// <summary>
    /// Key prefix for all cache entries (useful for multi-tenant scenarios)
    /// </summary>
    public string KeyPrefix { get; set; } = "meetlr";

    /// <summary>
    /// Maximum number of entries in memory cache (only for Memory provider)
    /// </summary>
    public int MemoryCacheMaxEntries { get; set; } = 1000;

    public TimeSpan GetDefaultDuration() => TimeSpan.FromSeconds(DefaultDurationSeconds);
}
