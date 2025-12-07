using MediatR;
using Meetlr.Application.Common.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Meetlr.Api.Endpoints.Auth.Register;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class Register : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ApplicationUrlsSettings _urlsSettings;

    public Register(IMediator mediator, IOptions<ApplicationUrlsSettings> urlsSettings)
    {
        _mediator = mediator;
        _urlsSettings = urlsSettings.Value;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Handle([FromBody] RegisterRequest request)
    {
        var command = RegisterRequest.ToCommand(request);
        var commandResponse = await _mediator.Send(command);
        var response = RegisterResponse.FromCommandResponse(commandResponse, _urlsSettings);
        return CreatedAtAction(nameof(Handle), new { id = response.UserId }, response);
    }
}
