using MediatR;
using Meetlr.Api.Models.Contacts;
using Meetlr.Application.Features.Contacts.Commands.UpdateContact;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Contacts;

[ApiController]
[Route("api/contacts")]
[Authorize]
public class UpdateContact : ControllerBase
{
    private readonly IMediator _mediator;

    public UpdateContact(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Update an existing contact
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ContactResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle([FromRoute] Guid id, [FromBody] UpdateContactRequest request)
    {
        var command = new UpdateContactCommand
        {
            Id = id,
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            TimeZone = request.TimeZone,
            Company = request.Company,
            JobTitle = request.JobTitle,
            ProfileImageUrl = request.ProfileImageUrl,
            PreferredLanguage = request.PreferredLanguage,
            Tags = request.Tags,
            MarketingConsent = request.MarketingConsent,
            IsShared = request.IsShared,
            IsBlacklisted = request.IsBlacklisted,
            BlockedReason = request.BlockedReason,
            CustomFieldsJson = request.CustomFieldsJson
        };

        var commandResponse = await _mediator.Send(command);

        var response = new ContactResponse
        {
            Id = commandResponse.Id,
            Name = commandResponse.Name,
            Email = commandResponse.Email,
            UpdatedAt = commandResponse.UpdatedAt
        };

        return Ok(response);
    }
}
