using MediatR;
using Meetlr.Api.Endpoints.Common;
using Meetlr.Application.Features.EmailTemplates.Commands.DeleteEmailTemplate;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.EmailTemplates.Delete;

[ApiController]
[Route("api/email-templates")]
public class DeleteEmailTemplateEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public DeleteEmailTemplateEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Delete an email template (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(DeleteEmailTemplateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Handle(Guid id)
    {
        var command = new DeleteEmailTemplateCommand { Id = id };
        var commandResponse = await _mediator.Send(command);
        var response = DeleteEmailTemplateResponse.FromCommandResponse(commandResponse);
        return Ok(response);
    }
}
