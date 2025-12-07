using MediatR;
using Meetlr.Api.Endpoints.Common;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.EmailTemplates.Copy;

[ApiController]
[Route("api/email-templates")]
public class CopyEmailTemplateEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public CopyEmailTemplateEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Copy a system or tenant template to user/tenant level for customization
    /// </summary>
    [HttpPost("copy")]
    [ProducesResponseType(typeof(CopyEmailTemplateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Handle([FromBody] CopyEmailTemplateRequest request)
    {
        var command = CopyEmailTemplateRequest.ToCommand(request, CurrentTenantId, CurrentUserId);
        var commandResponse = await _mediator.Send(command);
        var response = CopyEmailTemplateResponse.FromCommandResponse(commandResponse);
        return Ok(response);
    }
}
