using System.Text.Json;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Tenancy;
using Meetlr.Domain.Entities.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Meetlr.Infrastructure.Middleware;

/// <summary>
/// Middleware to resolve tenant context from various sources
/// Priority: 1) JWT claims, 2) Direct TenantId in URL, 3) Username in URL, 4) Entity ID in URL
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;

    // Routes that don't require tenant
    private static readonly HashSet<string> NoTenantRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/auth/",
        "/api/health",
        "/swagger",
        "/api/swagger",
        "/api/homepage-templates",
        "/api/homepage/templates"
    };

    public TenantResolutionMiddleware(RequestDelegate next, IMemoryCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        Guid? tenantId = null;
        string? subdomain = null;
        string? isAdminClaim = null;

        // ============================================
        // PRIORITY 1: JWT Claims (Authenticated Users)
        // ============================================
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = context.User.FindFirst("TenantId")?.Value;
            isAdminClaim = context.User.FindFirst("IsAdmin")?.Value;

            if (!string.IsNullOrEmpty(tenantIdClaim) && Guid.TryParse(tenantIdClaim, out var parsedTenantId))
            {
                tenantId = parsedTenantId;
                subdomain = await GetSubdomainByTenantIdAsync(tenantId.Value, unitOfWork);
                goto StoreAndProceed;
            }
        }

        // ============================================
        // PRIORITY 2: Direct TenantId in URL Path
        // Pattern: /api/public/tenant/{tenantId}/...
        // ============================================
        var directTenantId = ExtractDirectTenantIdFromPath(path);
        if (directTenantId.HasValue)
        {
            tenantId = directTenantId;
            subdomain = await GetSubdomainByTenantIdAsync(tenantId.Value, unitOfWork);
            goto StoreAndProceed;
        }

        // ============================================
        // PRIORITY 3: Username in URL Path
        // ============================================
        var username = ExtractUsernameFromPath(path);
        if (!string.IsNullOrEmpty(username))
        {
            (tenantId, subdomain) = await GetTenantByUsernameAsync(username, unitOfWork);
        }

        // ============================================
        // PRIORITY 4: Entity ID in URL Path (for availability, payments, etc.)
        // ============================================
        if (!tenantId.HasValue)
        {
            (tenantId, subdomain) = await ResolveFromEntityIdAsync(path, unitOfWork);
        }

    StoreAndProceed:
        // Store tenant context
        if (tenantId.HasValue)
        {
            context.Items["TenantId"] = tenantId.Value;
            context.Items["Subdomain"] = subdomain;
        }

        if (!string.IsNullOrEmpty(isAdminClaim))
        {
            context.Items["IsAdmin"] = isAdminClaim;
        }

        // Check if tenant is required
        if (!tenantId.HasValue && RequiresTenant(path))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                ErrorCode = "401-0-1",
                Message = "Authentication required. Please log in again.",
                HttpStatusCode = 401,
                TraceId = context.TraceIdentifier
            }));
            return;
        }

        await _next(context);
    }

    /// <summary>
    /// Extract direct TenantId from URL pattern: /api/public/tenant/{tenantId}/...
    /// </summary>
    private static Guid? ExtractDirectTenantIdFromPath(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // /api/public/tenant/{tenantId}/...
        if (segments.Length >= 4 && segments[0] == "api" && segments[1] == "public" && segments[2] == "tenant")
        {
            if (Guid.TryParse(segments[3], out var tenantId))
                return tenantId;
        }

        return null;
    }

    /// <summary>
    /// Extract username from URL patterns
    /// </summary>
    private static string? ExtractUsernameFromPath(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2) return null;

        // /book/{username} or /book/{username}/{slug}
        if (segments[0] == "book")
            return segments[1];

        // /site/{username}
        if (segments[0] == "site")
            return segments[1];

        // /booking/{username}
        if (segments[0] == "booking")
            return segments[1];

        // /api/public/homepage/{username}
        if (segments.Length >= 4 && segments[0] == "api" && segments[1] == "public" && segments[2] == "homepage")
            return segments[3];

        // /api/public/meetlrevents/username/{username}/slug/{slug}
        if (segments.Length >= 5 && segments[0] == "api" && segments[1] == "public" && segments[2] == "meetlrevents" && segments[3] == "username")
            return segments[4];

        // /api/public/analytics/username/{username}/eventSlug/{eventSlug}/track
        if (segments.Length >= 5 && segments[0] == "api" && segments[1] == "public" && segments[2] == "analytics" && segments[3] == "username")
            return segments[4];

        // Legacy patterns (keep for backwards compatibility)
        // /api/public-homepage/{username}
        if (segments.Length >= 3 && segments[0] == "api" && segments[1] == "public-homepage")
            return segments[2];

        // /api/booking/{username}/{event-slug}
        if (segments.Length >= 3 && segments[0] == "api" && segments[1] == "booking")
            return segments[2];

        // /api/meetlrevents/username/{username}
        if (segments.Length >= 4 && segments[0] == "api" && segments[1] == "meetlrevents" && segments[2] == "username")
            return segments[3];

        return null;
    }

    private async Task<(Guid?, string?)> GetTenantByUsernameAsync(string username, IUnitOfWork unitOfWork)
    {
        var cacheKey = $"tenant:username:{username.ToLower()}";

        var result = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
            entry.Size = 1;

            // Case-insensitive comparison since URL path is lowercased
            var user = await unitOfWork.Repository<User>()
                .GetQueryable()
                .IgnoreQueryFilters()
                .Where(u => u.MeetlrUsername != null && u.MeetlrUsername.ToLower() == username.ToLower() && !u.IsDeleted)
                .Select(u => new { u.TenantId, u.Tenant.Subdomain })
                .FirstOrDefaultAsync();

            return user;
        });

        return result != null ? (result.TenantId, result.Subdomain) : (null, null);
    }

    private async Task<(Guid?, string?)> ResolveFromEntityIdAsync(string path, IUnitOfWork unitOfWork)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // Handle analytics URL with eventSlug: /api/public/analytics/username/{username}/eventSlug/{eventSlug}/track
        if (segments.Length >= 7 && segments[0] == "api" && segments[1] == "public" && segments[2] == "analytics"
            && segments[3] == "username" && segments[5] == "eventslug")
        {
            var eventSlug = segments[6];
            if (eventSlug != "none")
            {
                var (tenantId, subdomain) = await GetTenantByEventSlugAsync(eventSlug, unitOfWork);
                if (tenantId.HasValue)
                    return (tenantId, subdomain);
            }
        }

        // Find first GUID in path (skip tenant GUID if in /tenant/{guid} pattern)
        Guid? entityId = null;
        for (int i = 0; i < segments.Length; i++)
        {
            // Skip the tenant segment
            if (i > 0 && segments[i - 1] == "tenant")
                continue;

            if (Guid.TryParse(segments[i], out var parsed))
            {
                entityId = parsed;
                break;
            }
        }

        if (!entityId.HasValue) return (null, null);

        // Determine entity type from path and lookup tenant
        Guid? tenantId2 = null;

        // /api/public/availability/meetrlevent/{meetlrEventId}/... - resolve from MeetlrEvent
        if (path.Contains("/availability/meetrlevent/") || path.Contains("/availability/meetlrevent/"))
        {
            tenantId2 = await GetTenantByEntityAsync("MeetlrEvent", entityId.Value, unitOfWork);
        }
        else if (path.Contains("/availability/") || path.Contains("/meetlrevents/"))
        {
            tenantId2 = await GetTenantByEntityAsync("MeetlrEvent", entityId.Value, unitOfWork);
        }
        else if (path.Contains("/schedule/"))
        {
            tenantId2 = await GetTenantByEntityAsync("Schedule", entityId.Value, unitOfWork);
        }
        else if (path.Contains("/payments/") && path.Contains("/booking"))
        {
            tenantId2 = await GetTenantByEntityAsync("Booking", entityId.Value, unitOfWork);
        }
        else if (path.Contains("/payments/") && path.Contains("/series"))
        {
            tenantId2 = await GetTenantByEntityAsync("BookingSeries", entityId.Value, unitOfWork);
        }
        else if (path.Contains("/bookings/") && path.Contains("/reschedule"))
        {
            tenantId2 = await GetTenantByEntityAsync("Booking", entityId.Value, unitOfWork);
        }

        if (!tenantId2.HasValue) return (null, null);

        var subdomain2 = await GetSubdomainByTenantIdAsync(tenantId2.Value, unitOfWork);
        return (tenantId2, subdomain2);
    }

    private async Task<(Guid?, string?)> GetTenantByEventSlugAsync(string eventSlug, IUnitOfWork unitOfWork)
    {
        var cacheKey = $"tenant:eventslug:{eventSlug.ToLower()}";

        var tenantId = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
            entry.Size = 1;

            return await unitOfWork.Repository<Domain.Entities.Events.MeetlrEvent>()
                .GetQueryable()
                .IgnoreQueryFilters()
                .Where(e => e.Slug == eventSlug && !e.IsDeleted)
                .Select(e => (Guid?)e.TenantId)
                .FirstOrDefaultAsync();
        });

        if (!tenantId.HasValue) return (null, null);

        var subdomain = await GetSubdomainByTenantIdAsync(tenantId.Value, unitOfWork);
        return (tenantId, subdomain);
    }

    private async Task<Guid?> GetTenantByEntityAsync(string entityType, Guid entityId, IUnitOfWork unitOfWork)
    {
        var cacheKey = $"tenant:entity:{entityType}:{entityId}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            entry.Size = 1;

            return entityType switch
            {
                "MeetlrEvent" => await unitOfWork.Repository<Domain.Entities.Events.MeetlrEvent>()
                    .GetQueryable().IgnoreQueryFilters()
                    .Where(e => e.Id == entityId).Select(e => (Guid?)e.TenantId).FirstOrDefaultAsync(),

                "Schedule" => await unitOfWork.Repository<Domain.Entities.Scheduling.AvailabilitySchedule>()
                    .GetQueryable().IgnoreQueryFilters()
                    .Where(s => s.Id == entityId).Select(s => (Guid?)s.TenantId).FirstOrDefaultAsync(),

                "Booking" => await unitOfWork.Repository<Domain.Entities.Events.Booking>()
                    .GetQueryable().IgnoreQueryFilters()
                    .Where(b => b.Id == entityId).Select(b => (Guid?)b.TenantId).FirstOrDefaultAsync(),

                "BookingSeries" => await unitOfWork.Repository<Domain.Entities.Events.BookingSeries>()
                    .GetQueryable().IgnoreQueryFilters()
                    .Where(s => s.Id == entityId).Select(s => (Guid?)s.TenantId).FirstOrDefaultAsync(),

                _ => null
            };
        });
    }

    private async Task<string?> GetSubdomainByTenantIdAsync(Guid tenantId, IUnitOfWork unitOfWork)
    {
        var cacheKey = $"tenant:subdomain:{tenantId}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
            entry.Size = 1;

            return await unitOfWork.Repository<Tenant>()
                .GetQueryable()
                .IgnoreQueryFilters()
                .Where(t => t.Id == tenantId && t.IsActive)
                .Select(t => t.Subdomain)
                .FirstOrDefaultAsync();
        });
    }

    private static bool RequiresTenant(string path)
    {
        // Non-API routes don't require tenant
        if (!path.StartsWith("/api/"))
            return false;

        // Check whitelist
        foreach (var route in NoTenantRoutes)
        {
            if (path.StartsWith(route, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        // All /api/public/ routes resolve tenant themselves (from URL params or entity IDs)
        if (path.StartsWith("/api/public/"))
            return false;

        // Legacy patterns (keep for backwards compatibility)
        if (path.StartsWith("/api/public-homepage/"))
            return false;
        if (path.StartsWith("/api/meetlrevents/username/"))
            return false;

        // Reschedule endpoints are public (token-based auth)
        if (path.Contains("/reschedule"))
            return false;

        return true;
    }
}
