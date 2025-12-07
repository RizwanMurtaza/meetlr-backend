using MediatR;
using Meetlr.Module.Analytics.Application.Queries.GetPlatformAnalytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Analytics.Endpoints.GetPlatformAnalytics;

/// <summary>
/// Endpoint for getting platform-wide analytics (admin only)
/// </summary>
[ApiController]
[Route("api/analytics/platform")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class GetPlatformAnalyticsEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public GetPlatformAnalyticsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get platform-wide analytics (admin only)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetPlatformAnalyticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Handle([FromQuery] GetPlatformAnalyticsRequest request)
    {
        var tenantId = GetCurrentTenantId();

        var query = new GetPlatformAnalyticsQuery
        {
            TenantId = tenantId,
            Period = request.Period
        };

        var queryResponse = await _mediator.Send(query);
        var response = GetPlatformAnalyticsResponse.FromQueryResponse(queryResponse);

        return Ok(response);
    }

    private Guid? GetCurrentTenantId()
    {
        if (HttpContext.Items.TryGetValue("TenantId", out var tenantId) && tenantId is Guid guid)
            return guid;
        return null;
    }
}
