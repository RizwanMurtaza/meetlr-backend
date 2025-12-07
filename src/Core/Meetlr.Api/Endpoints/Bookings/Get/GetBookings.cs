using MediatR;
using Meetlr.Api.Endpoints.Common;
using Meetlr.Application.Features.Bookings.Queries.GetBookings;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Bookings.Get;

[Route("api/bookings")]
public class GetBookings : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public GetBookings(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(GetBookingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var userId = CurrentUserId;

        var query = new GetBookingsQuery
        {
            UserId = userId,
            StartDate = startDate,
            EndDate = endDate
        };

        var queryResponse = await _mediator.Send(query);
        var response = GetBookingsResponse.FromQueryResponse(queryResponse);

        return Ok(response);
    }
}
