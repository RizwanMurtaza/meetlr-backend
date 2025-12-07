using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Tenants.Signup;

[ApiController]
[Route("api/tenants")]
[AllowAnonymous]
public class TenantSignup : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;

    public TenantSignup(IMediator mediator, IConfiguration configuration)
    {
        _mediator = mediator;
        _configuration = configuration;
    }

    /// <summary>
    /// Create a new tenant with an admin user (Signup)
    /// </summary>
    [HttpPost("signup")]
    [ProducesResponseType(typeof(TenantSignupResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Handle([FromBody] TenantSignupRequest request)
    {
        var command = TenantSignupRequest.ToCommand(request);
        var commandResponse = await _mediator.Send(command);

        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "mywebsite.com";
        var response = TenantSignupResponse.FromCommandResponse(commandResponse, baseUrl);

        return CreatedAtAction(nameof(Handle), new { subdomain = response.Subdomain }, response);
    }
}
