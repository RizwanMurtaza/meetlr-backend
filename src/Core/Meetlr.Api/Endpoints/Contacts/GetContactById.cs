using MediatR;
using Meetlr.Api.Models.Contacts;
using Meetlr.Application.Features.Contacts.Queries.GetContactById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Contacts;

[ApiController]
[Route("api/contacts")]
[Authorize]
public class GetContactById : ControllerBase
{
    private readonly IMediator _mediator;

    public GetContactById(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get a contact by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContactDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle([FromRoute] Guid id)
    {
        var query = new GetContactByIdQuery { Id = id };
        var queryResponse = await _mediator.Send(query);

        if (queryResponse.Contact == null)
        {
            return NotFound();
        }

        var response = new ContactDetailResponse
        {
            Id = queryResponse.Contact.Id,
            UserId = queryResponse.Contact.UserId,
            Name = queryResponse.Contact.Name,
            Email = queryResponse.Contact.Email,
            Phone = queryResponse.Contact.Phone,
            TimeZone = queryResponse.Contact.TimeZone,
            Company = queryResponse.Contact.Company,
            JobTitle = queryResponse.Contact.JobTitle,
            ProfileImageUrl = queryResponse.Contact.ProfileImageUrl,
            PreferredLanguage = queryResponse.Contact.PreferredLanguage,
            Tags = queryResponse.Contact.Tags,
            Source = queryResponse.Contact.Source.ToString(),
            IsShared = queryResponse.Contact.IsShared,
            IsBlacklisted = queryResponse.Contact.IsBlacklisted,
            BlockedReason = queryResponse.Contact.BlockedReason,
            MarketingConsent = queryResponse.Contact.MarketingConsent,
            TotalBookings = queryResponse.Contact.TotalBookings,
            NoShowCount = queryResponse.Contact.NoShowCount,
            LastContactedAt = queryResponse.Contact.LastContactedAt,
            CustomFieldsJson = queryResponse.Contact.CustomFieldsJson,
            CreatedAt = queryResponse.Contact.CreatedAt,
            UpdatedAt = queryResponse.Contact.UpdatedAt
        };

        return Ok(response);
    }
}
