using MediatR;
using Meetlr.Api.Endpoints.Common;
using Meetlr.Application.Features.MeetlrEvents.Commands.DeleteEmailTemplate;
using Meetlr.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.EventEmailTemplates.Delete;

[Route("api/meetlr-events/{eventId}/email-templates")]
public class DeleteEventEmailTemplate : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public DeleteEventEmailTemplate(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Delete a custom email template (revert to default)
    /// </summary>
    [HttpDelete("{templateType}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle([FromRoute] Guid eventId, [FromRoute] EmailTemplateType templateType)
    {
        var command = new DeleteEventEmailTemplateCommand
        {
            MeetlrEventId = eventId,
            TemplateType = templateType
        };

        var result = await _mediator.Send(command);

        if (result)
            return NoContent();

        return NotFound();
    }
}
