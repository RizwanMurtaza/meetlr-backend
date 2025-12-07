using Meetlr.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Meetlr.Infrastructure.Services;

/// <summary>
/// Service for resolving and accessing tenant context from HTTP request
/// </summary>
public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? TenantId
    {
        get
        {
            var tenantId = _httpContextAccessor.HttpContext?.Items["TenantId"] as Guid?;
            return tenantId;
        }
    }

    public string? Subdomain
    {
        get
        {
            var subdomain = _httpContextAccessor.HttpContext?.Items["Subdomain"] as string;
            return subdomain;
        }
    }

    public bool IsTenantResolved => TenantId.HasValue;

    public bool IsAdmin
    {
        get
        {
            var isAdminValue = _httpContextAccessor.HttpContext?.Items["IsAdmin"] as string;
            return !string.IsNullOrEmpty(isAdminValue) && bool.TryParse(isAdminValue, out var isAdmin) && isAdmin;
        }
    }
}
