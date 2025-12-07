using System.Security.Claims;
using MediatR;
using Meetlr.Module.Analytics.Application.Queries.GetUserAnalytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Analytics.Endpoints.GetUserAnalytics;

/// <summary>
/// Endpoint for getting user analytics dashboard data
/// </summary>
[ApiController]
[Route("api/analytics")]
[Authorize]
public class GetUserAnalyticsEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public GetUserAnalyticsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get analytics dashboard data for the current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetUserAnalyticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle([FromQuery] GetUserAnalyticsRequest request)
    {
        var userId = GetCurrentUserId();
        var username = GetCurrentUsername();

        var query = new GetUserAnalyticsQuery
        {
            UserId = userId,
            Username = username,
            Period = request.Period
        };

        var queryResponse = await _mediator.Send(query);
        var response = GetUserAnalyticsResponse.FromQueryResponse(queryResponse);

        return Ok(response);
    }

    private Guid GetCurrentUserId()
    {
        var userIdString = User.FindFirstValue("MeetlrUserId");
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found");
        return userId;
    }

    private string GetCurrentUsername()
    {
        return User.FindFirstValue("username") ?? User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
    }
}
