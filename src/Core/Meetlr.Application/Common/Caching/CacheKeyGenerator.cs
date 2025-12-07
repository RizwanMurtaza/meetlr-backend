using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Meetlr.Application.Common.Caching;

/// <summary>
/// Generates consistent cache keys based on query type and parameters
/// </summary>
public static class CacheKeyGenerator
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Generates a cache key for a query based on its type and properties
    /// Format: "query:{queryType}:{userId}:{hash}"
    /// </summary>
    public static string GenerateKey<TQuery>(TQuery query, Guid? userId) where TQuery : class
    {
        var queryType = typeof(TQuery).Name;
        var queryJson = JsonSerializer.Serialize(query, SerializerOptions);
        var hash = GenerateHash(queryJson);

        var userPart = userId.HasValue ? userId.Value.ToString() : "anonymous";
        return $"query:{queryType}:{userPart}:{hash}";
    }

    /// <summary>
    /// Generates a user-specific cache key prefix
    /// Format: "user:{userId}"
    /// </summary>
    public static string GenerateUserPrefix(Guid userId)
    {
        return $"user:{userId}";
    }

    /// <summary>
    /// Generates a tenant-specific cache key prefix
    /// Format: "tenant:{tenantId}"
    /// </summary>
    public static string GenerateTenantPrefix(Guid tenantId)
    {
        return $"tenant:{tenantId}";
    }

    /// <summary>
    /// Generates a resource-specific cache key prefix
    /// Format: "{resource}:{userId}"
    /// </summary>
    public static string GenerateResourcePrefix(string resource, Guid userId)
    {
        return $"{resource}:{userId}";
    }

    /// <summary>
    /// Replaces placeholders in cache key templates
    /// Supports: {userId}, {tenantId}
    /// </summary>
    public static string ReplaceKeyPlaceholders(string template, Guid? userId, Guid? tenantId)
    {
        var result = template;

        if (userId.HasValue)
            result = result.Replace("{userId}", userId.Value.ToString());

        if (tenantId.HasValue)
            result = result.Replace("{tenantId}", tenantId.Value.ToString());

        return result;
    }

    private static string GenerateHash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash)[..16]; // Use first 16 chars for brevity
    }
}
