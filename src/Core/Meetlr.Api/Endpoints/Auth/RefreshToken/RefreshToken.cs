using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Auth.RefreshToken;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class RefreshToken : ControllerBase
{
    private readonly IMediator _mediator;

    public RefreshToken(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Refresh access token using a valid refresh token
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle([FromBody] RefreshTokenRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var deviceInfo = Request.Headers.UserAgent.ToString();

        var command = RefreshTokenRequest.ToCommand(request, ipAddress, deviceInfo);
        var commandResponse = await _mediator.Send(command);
        var response = RefreshTokenResponse.FromCommandResponse(commandResponse);
        return Ok(response);
    }
}
