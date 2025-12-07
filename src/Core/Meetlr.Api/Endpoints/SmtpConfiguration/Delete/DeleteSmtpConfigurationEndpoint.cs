using MediatR;
using Meetlr.Api.Endpoints.Common;
using Meetlr.Application.Features.SmtpConfiguration.Commands.DeleteSmtpConfiguration;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.SmtpConfiguration.Delete;

[ApiController]
[Route("api/smtp-configuration")]
public class DeleteSmtpConfigurationEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public DeleteSmtpConfigurationEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Delete an SMTP configuration (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(DeleteSmtpConfigurationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Handle(Guid id)
    {
        var command = new DeleteSmtpConfigurationCommand { Id = id };
        var commandResponse = await _mediator.Send(command);
        var response = DeleteSmtpConfigurationResponse.FromCommandResponse(commandResponse);
        return Ok(response);
    }
}
