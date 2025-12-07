using MediatR;
using Meetlr.Application.Features.Contacts.Commands.DeleteContact;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Contacts;

[ApiController]
[Route("api/contacts")]
[Authorize]
public class DeleteContact : ControllerBase
{
    private readonly IMediator _mediator;

    public DeleteContact(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Delete a contact (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle([FromRoute] Guid id)
    {
        var command = new DeleteContactCommand { Id = id };
        await _mediator.Send(command);

        return NoContent();
    }
}
