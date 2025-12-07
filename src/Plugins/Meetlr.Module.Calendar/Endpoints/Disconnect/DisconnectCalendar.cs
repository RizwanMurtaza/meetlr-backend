using MediatR;
using Meetlr.Module.Calendar.Application.Commands.DisconnectCalendar;
using Meetlr.Module.Calendar.Endpoints.Common;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Calendar.Endpoints.Disconnect;

[Route("api/calendar")]
public class DisconnectCalendar : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public DisconnectCalendar(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpDelete("{calendarIntegrationId}")]
    public async Task<IActionResult> Handle(Guid calendarIntegrationId)
    {
        var command = new DisconnectCalendarCommand
        {
            CalendarIntegrationId = calendarIntegrationId,
            UserId = CurrentUserId
        };

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return NotFound(new { error = result.Message });
        }

        return Ok(new DisconnectCalendarResponse
        {
            Success = true,
            Message = result.Message
        });
    }
}
