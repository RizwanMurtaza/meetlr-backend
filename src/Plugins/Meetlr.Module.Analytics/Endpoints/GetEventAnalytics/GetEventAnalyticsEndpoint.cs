using System.Security.Claims;
using MediatR;
using Meetlr.Module.Analytics.Application.Queries.GetEventAnalytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Analytics.Endpoints.GetEventAnalytics;

/// <summary>
/// Endpoint for getting analytics for a specific event
/// </summary>
[ApiController]
[Route("api/analytics/events")]
[Authorize]
public class GetEventAnalyticsEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public GetEventAnalyticsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get analytics for a specific event
    /// </summary>
    [HttpGet("{eventId:guid}")]
    [ProducesResponseType(typeof(GetEventAnalyticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid eventId,
        [FromQuery] string eventSlug,
        [FromQuery] GetEventAnalyticsRequest request)
    {
        var username = GetCurrentUsername();

        var query = new GetEventAnalyticsQuery
        {
            EventId = eventId,
            EventSlug = eventSlug,
            Username = username,
            Period = request.Period
        };

        var queryResponse = await _mediator.Send(query);
        var response = GetEventAnalyticsResponse.FromQueryResponse(queryResponse);

        return Ok(response);
    }

    private string GetCurrentUsername()
    {
        return User.FindFirstValue("username") ?? User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
    }
}
