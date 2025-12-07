using MediatR;
using Meetlr.Api.Endpoints.Common;
using Meetlr.Application.Features.Dashboard.Queries.GetDashboardStats;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Dashboard.GetStats;

[Route("api/dashboard")]
public class GetDashboardStats : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public GetDashboardStats(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("stats")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(GetDashboardStatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle()
    {
        var query = new GetDashboardStatsQuery
        {
            UserId = CurrentUserId
        };

        var queryResponse = await _mediator.Send(query);
        var response = GetDashboardStatsResponse.FromQueryResponse(queryResponse);

        return Ok(response);
    }
}
