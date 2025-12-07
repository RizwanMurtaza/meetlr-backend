using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Analytics.Endpoints.Public.TrackPageView;

/// <summary>
/// Endpoint for tracking page views (public, no auth required)
/// </summary>
[ApiController]
[Route("api/public/analytics")]
[AllowAnonymous]
public class TrackPageViewEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public TrackPageViewEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Track a page view event
    /// </summary>
    /// <remarks>
    /// This endpoint is public and does not require authentication.
    /// It's designed for fire-and-forget tracking from the frontend.
    /// The username is extracted from the URL path to resolve the tenant.
    /// Use "none" for eventSlug when tracking homepage or event list pages.
    /// </remarks>
    [HttpPost("username/{username}/eventSlug/{eventSlug}/track")]
    [ProducesResponseType(typeof(TrackPageViewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Handle(
        [FromRoute] string username,
        [FromRoute] string eventSlug,
        [FromBody] TrackPageViewRequest request)
    {
        // Get IP address from request (for hashing)
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        // Convert "none" to null for non-event page tracking
        var actualEventSlug = eventSlug == "none" ? null : eventSlug;

        var command = request.ToCommand(username, actualEventSlug, ipAddress);
        var commandResponse = await _mediator.Send(command);
        var response = TrackPageViewResponse.FromCommandResponse(commandResponse);

        return Ok(response);
    }
}
