using MediatR;
using Meetlr.Api.Endpoints.Common;
using Meetlr.Application.Features.Schedule.Queries.GetAvailabilitySchedules;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Schedule.Get;

[Route("api/schedule")]
public class GetAvailabilitySchedules : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public GetAvailabilitySchedules(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(GetAvailabilitySchedulesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle()
    {
        var userId = CurrentUserId;

        var query = new GetAvailabilitySchedulesQuery
        {
            UserId = userId
        };

        var queryResponse = await _mediator.Send(query);
        var response = GetAvailabilitySchedulesResponse.FromQueryResponse(queryResponse);

        return Ok(response);
    }
}
