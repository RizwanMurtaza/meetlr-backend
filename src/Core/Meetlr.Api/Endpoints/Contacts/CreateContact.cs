using MediatR;
using Meetlr.Api.Models.Contacts;
using Meetlr.Application.Features.Contacts.Commands.CreateContact;
using Meetlr.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Contacts;

[ApiController]
[Route("api/contacts")]
[Authorize]
public class CreateContact : ControllerBase
{
    private readonly IMediator _mediator;

    public CreateContact(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new contact
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ContactResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle([FromBody] CreateContactRequest request)
    {
        var command = new CreateContactCommand
        {
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
            Source = Enum.TryParse<ContactSource>(request.Source, out var source) ? source : ContactSource.Manual,
            IsShared = request.IsShared,
            CustomFieldsJson = request.CustomFieldsJson
        };

        var commandResponse = await _mediator.Send(command);

        var response = new ContactResponse
        {
            Id = commandResponse.Id,
            Name = commandResponse.Name,
            Email = commandResponse.Email,
            CreatedAt = commandResponse.CreatedAt
        };

        return CreatedAtAction(nameof(GetContactById.Handle), "GetContactById", new { id = response.Id }, response);
    }
}
