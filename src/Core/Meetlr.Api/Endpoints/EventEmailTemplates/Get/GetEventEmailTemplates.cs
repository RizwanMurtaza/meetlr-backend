using MediatR;
using Meetlr.Api.Endpoints.Common;
using Meetlr.Application.Features.MeetlrEvents.Queries.GetEmailTemplates;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.EventEmailTemplates.Get;

[Route("api/meetlr-events/{eventId}/email-templates")]
public class GetEventEmailTemplates : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public GetEventEmailTemplates(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all email templates for a Meetlr event (custom + defaults)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<GetEventEmailTemplatesResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle([FromRoute] Guid eventId)
    {
        var query = new GetEventEmailTemplatesQuery { MeetlrEventId = eventId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
