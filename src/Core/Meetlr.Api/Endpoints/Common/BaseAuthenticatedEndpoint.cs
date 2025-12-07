using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Common;

/// <summary>
/// Base controller for authenticated endpoints that automatically extracts and validates the user ID from claims.
/// Eliminates duplicate code for UserId extraction across all endpoints.
/// </summary>
[ApiController]
[Authorize]
public abstract class BaseAuthenticatedEndpoint : ControllerBase
{
    /// <summary>
    /// Gets the current authenticated user's ID from the MeetlrUserId claim.
    /// Throws UnauthorizedAccessException if the claim is missing or invalid.
    /// [Authorize] attribute ensures this should always succeed.
    /// </summary>
    protected Guid CurrentUserId
    {
        get
        {
            var userIdString = User.FindFirstValue("MeetlrUserId");
            if (string.IsNullOrEmpty(userIdString))
                throw new UnauthorizedAccessException("MeetlrUserId claim is missing");

            if (!Guid.TryParse(userIdString, out var userId))
                throw new UnauthorizedAccessException("MeetlrUserId claim is invalid");

            return userId;
        }
    }

    /// <summary>
    /// Gets the tenant ID from the HttpContext items (set by TenantResolutionMiddleware).
    /// Returns null if no tenant context is available.
    /// </summary>
    protected Guid? CurrentTenantId
    {
        get
        {
            if (HttpContext.Items.TryGetValue("TenantId", out var tenantId) && tenantId is Guid guid)
                return guid;

            return null;
        }
    }

    /// <summary>
    /// Gets the subdomain from the HttpContext items (set by TenantResolutionMiddleware).
    /// Returns null if no tenant context is available.
    /// </summary>
    protected string? CurrentSubdomain
    {
        get
        {
            if (HttpContext.Items.TryGetValue("Subdomain", out var subdomain) && subdomain is string subdomainStr)
                return subdomainStr;

            return null;
        }
    }
}
