using MediatR;
using Meetlr.Api.Endpoints.Common;
using Meetlr.Application.Features.SmtpConfiguration.Commands.SetActiveSmtpConfiguration;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.SmtpConfiguration.SetActive;

[ApiController]
[Route("api/smtp-configuration")]
public class SetActiveSmtpConfigurationEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public SetActiveSmtpConfigurationEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Set an SMTP configuration as active
    /// </summary>
    [HttpPost("{id:guid}/set-active")]
    [ProducesResponseType(typeof(SetActiveSmtpConfigurationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle(Guid id)
    {
        var command = new SetActiveSmtpConfigurationCommand { Id = id };
        var commandResponse = await _mediator.Send(command);
        var response = SetActiveSmtpConfigurationResponse.FromCommandResponse(commandResponse);
        return Ok(response);
    }
}
