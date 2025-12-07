using MediatR;
using Meetlr.Api.Models.Contacts;
using Meetlr.Application.Features.Contacts.Queries.GetAllContacts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Contacts;

[ApiController]
[Route("api/contacts")]
[Authorize]
public class GetAllContacts : ControllerBase
{
    private readonly IMediator _mediator;

    public GetAllContacts(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all contacts with pagination and filtering
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedContactsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isBlacklisted = null,
        [FromQuery] string? source = null,
        [FromQuery] string? sortBy = "LastContactedAt",
        [FromQuery] bool sortDescending = true)
    {
        var query = new GetAllContactsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            IsBlacklisted = isBlacklisted,
            Source = source,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var queryResponse = await _mediator.Send(query);

        var response = new PaginatedContactsResponse
        {
            Contacts = queryResponse.Contacts.Select(c => new ContactResponse
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                TimeZone = c.TimeZone,
                Company = c.Company,
                JobTitle = c.JobTitle,
                ProfileImageUrl = c.ProfileImageUrl,
                PreferredLanguage = c.PreferredLanguage,
                Tags = c.Tags,
                Source = c.Source.ToString(),
                IsShared = c.IsShared,
                IsBlacklisted = c.IsBlacklisted,
                BlockedReason = c.BlockedReason,
                MarketingConsent = c.MarketingConsent,
                TotalBookings = c.TotalBookings,
                NoShowCount = c.NoShowCount,
                LastContactedAt = c.LastContactedAt,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToList(),
            TotalCount = queryResponse.TotalCount,
            PageNumber = queryResponse.PageNumber,
            PageSize = queryResponse.PageSize,
            TotalPages = queryResponse.TotalPages,
            HasPreviousPage = queryResponse.HasPreviousPage,
            HasNextPage = queryResponse.HasNextPage
        };

        return Ok(response);
    }
}
