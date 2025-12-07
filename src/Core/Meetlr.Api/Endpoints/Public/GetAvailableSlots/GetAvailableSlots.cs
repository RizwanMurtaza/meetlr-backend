using MediatR;
using Meetlr.Application.Features.Availability.Queries.GetAvailableSlots;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Public.GetAvailableSlots;

[ApiController]
[Route("api/public/availability/meetrlevent")]
[AllowAnonymous]
public class GetAvailableSlots : ControllerBase
{
    private readonly IMediator _mediator;

    public GetAvailableSlots(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get available time slots for an event type
    /// </summary>
    [HttpPost("{meetlrEventId}/available-slots")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(GetAvailableSlotsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid meetlrEventId,
        [FromBody] GetAvailableSlotsRequest request)
    {
        var query = new GetAvailableSlotsQuery
        {
            MeetlrEventId = meetlrEventId,
            UserSlug = request.UserSlug,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TimeZone = request.TimeZone
        };

        var queryResponse = await _mediator.Send(query);
        var response = GetAvailableSlotsResponse.FromQueryResponse(queryResponse);

        return Ok(response);
    }
}
