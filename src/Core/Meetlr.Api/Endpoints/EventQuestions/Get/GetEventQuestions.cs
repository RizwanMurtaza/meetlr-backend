using MediatR;
using Meetlr.Application.Features.MeetlrEvents.Queries.GetQuestions;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.EventQuestions.Get;

[Route("api/meetlr-events/{eventId}/questions")]
[ApiController]
public class GetEventQuestions : ControllerBase
{
    private readonly IMediator _mediator;

    public GetEventQuestions(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get questions for a Meetlr event (public endpoint for booking pages)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetEventQuestionsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handle([FromRoute] Guid eventId)
    {
        var query = new GetEventQuestionsQuery { MeetlrEventId = eventId };
        var queryResponse = await _mediator.Send(query);
        var response = GetEventQuestionsResponse.FromQueryResponse(queryResponse);

        return Ok(response);
    }
}
