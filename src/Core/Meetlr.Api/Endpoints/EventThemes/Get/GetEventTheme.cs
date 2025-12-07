using MediatR;
using Meetlr.Application.Features.MeetlrEvents.Queries.GetTheme;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.EventThemes.Get;

[Route("api/meetlr-events/{eventId}/theme")]
[ApiController]
public class GetEventTheme : ControllerBase
{
    private readonly IMediator _mediator;

    public GetEventTheme(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get the theme for a Meetlr event (public endpoint for booking pages)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetEventThemeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle([FromRoute] Guid eventId)
    {
        var query = new GetEventThemeQuery { MeetlrEventId = eventId };
        var queryResponse = await _mediator.Send(query);

        if (queryResponse == null)
        {
            // Return default theme if none exists
            return Ok(GetEventThemeResponse.GetDefault(eventId));
        }

        var response = GetEventThemeResponse.FromQueryResponse(queryResponse);
        return Ok(response);
    }
}
