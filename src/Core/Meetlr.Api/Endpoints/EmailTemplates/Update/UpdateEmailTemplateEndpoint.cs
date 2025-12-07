using MediatR;
using Meetlr.Api.Endpoints.Common;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.EmailTemplates.Update;

[ApiController]
[Route("api/email-templates")]
public class UpdateEmailTemplateEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public UpdateEmailTemplateEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Update an email template
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UpdateEmailTemplateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Handle(Guid id, [FromBody] UpdateEmailTemplateRequest request)
    {
        var command = UpdateEmailTemplateRequest.ToCommand(request, id);
        var commandResponse = await _mediator.Send(command);
        var response = UpdateEmailTemplateResponse.FromCommandResponse(commandResponse);
        return Ok(response);
    }
}
