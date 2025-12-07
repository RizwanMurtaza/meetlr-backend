using MediatR;
using Meetlr.Module.Calendar.Application.Queries.GetConnectedCalendars;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Calendar.Endpoints.GetConnected;

[ApiController]
[Route("api/calendar")]
[Authorize]
public class GetConnectedCalendars : ControllerBase
{
    private readonly ISender _sender;

    public GetConnectedCalendars(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("integrations")]
    [ProducesResponseType(typeof(GetConnectedCalendarsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetIntegrations([FromQuery] Guid scheduleId)
    {
        var query = new GetConnectedCalendarsQuery(scheduleId);
        var result = await _sender.Send(query);
        return Ok(result);
    }
}
