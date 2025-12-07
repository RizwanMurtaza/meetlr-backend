using MediatR;
using Meetlr.Api.Endpoints.Common;
using Meetlr.Application.Features.MeetlrEvents.Commands.SaveEmailTemplate;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.EventEmailTemplates.Save;

[Route("api/meetlr-events/{eventId}/email-templates")]
public class SaveEventEmailTemplate : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public SaveEventEmailTemplate(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Save a custom email template for a Meetlr event
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SaveEventEmailTemplateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle([FromRoute] Guid eventId, [FromBody] SaveEventEmailTemplateRequest request)
    {
        var command = new SaveEventEmailTemplateCommand
        {
            MeetlrEventId = eventId,
            TemplateType = request.TemplateType,
            Subject = request.Subject,
            HtmlBody = request.HtmlBody,
            IsActive = request.IsActive
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
