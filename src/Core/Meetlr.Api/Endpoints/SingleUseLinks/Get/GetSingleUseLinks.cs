using MediatR;
using Meetlr.Api.Endpoints.Common;
using Meetlr.Application.Features.MeetlrEvents.Queries.GetSingleUseLinks;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.SingleUseLinks.Get;

[Route("api/single-use-links")]
public class GetSingleUseLinks : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public GetSingleUseLinks(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all single-use booking links for a specific event type
    /// </summary>
    [HttpGet("event/{meetlrEventId:guid}")]
    [ProducesResponseType(typeof(GetSingleUseLinksResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid meetlrEventId,
        [FromQuery] bool? includeUsed = false)
    {
        var query = new GetSingleUseLinksQuery
        {
            MeetlrEventId = meetlrEventId,
            IncludeUsed = includeUsed
        };

        var queryResponse = await _mediator.Send(query);
        var response = GetSingleUseLinksResponse.FromQueryResponse(queryResponse);

        return Ok(response);
    }
}
