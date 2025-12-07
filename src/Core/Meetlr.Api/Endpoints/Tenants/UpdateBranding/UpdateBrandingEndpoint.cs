using MediatR;
using Meetlr.Application.Features.Tenants.Commands.UpdateBranding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Tenants.UpdateBranding;

[ApiController]
[Route("api/tenants")]
[Authorize]
public class UpdateBrandingEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public UpdateBrandingEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Update tenant branding (Admin only)
    /// </summary>
    [HttpPut("{tenantId}/branding")]
    [ProducesResponseType(typeof(UpdateBrandingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle(Guid tenantId, [FromBody] UpdateBrandingRequest request)
    {
        var command = UpdateBrandingRequest.ToCommand(request, tenantId);
        var response = await _mediator.Send(command);
        return Ok(response);
    }
}
