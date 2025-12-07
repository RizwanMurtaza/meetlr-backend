using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Auth.Login;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class Login : ControllerBase
{
    private readonly IMediator _mediator;

    public Login(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// User login
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle([FromBody] LoginRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var deviceInfo = Request.Headers.UserAgent.ToString();

        var command = LoginRequest.ToCommand(request, ipAddress, deviceInfo);
        var commandResponse = await _mediator.Send(command);
        var response = LoginResponse.FromCommandResponse(commandResponse);
        return Ok(response);
    }
}
