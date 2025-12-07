using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Calendar.Endpoints.Common;

/// <summary>
/// Base controller for authenticated endpoints that automatically extracts and validates the user ID from claims.
/// </summary>
[ApiController]
[Authorize]
public abstract class BaseAuthenticatedEndpoint : ControllerBase
{
    /// <summary>
    /// Gets the current authenticated user's ID from the MeetlrUserId claim.
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
    /// Gets the tenant ID from the HttpContext items.
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
}
