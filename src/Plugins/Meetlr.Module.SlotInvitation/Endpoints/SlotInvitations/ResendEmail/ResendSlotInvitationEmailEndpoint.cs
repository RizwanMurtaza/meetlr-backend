using MediatR;
using Meetlr.Module.SlotInvitation.Application.Commands.ResendSlotInvitationEmail;
using Meetlr.Module.SlotInvitation.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.SlotInvitation.Endpoints.SlotInvitations.ResendEmail;

[Route("api/slot-invitations")]
public class ResendSlotInvitationEmailEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public ResendSlotInvitationEmailEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Resend the invitation email for a slot invitation.
    /// Limited to 3 total attempts per invitation.
    /// </summary>
    [HttpPost("{id:guid}/resend-email")]
    [ProducesResponseType(typeof(ResendSlotInvitationEmailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle([FromRoute] Guid id)
    {
        var command = new ResendSlotInvitationEmailCommand
        {
            SlotInvitationId = id
        };

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(new ResendSlotInvitationEmailResponse
        {
            Success = result.Success,
            Message = result.Message ?? "Email queued for resend",
            EmailAttempts = result.EmailAttempts,
            CanResendAgain = result.CanResendAgain
        });
    }
}

public class ResendSlotInvitationEmailResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int EmailAttempts { get; set; }
    public bool CanResendAgain { get; set; }
}
