using Meetlr.Domain.Entities.Contacts;
using Meetlr.Domain.Enums;
using MediatR;

namespace Meetlr.Application.Features.Contacts.Queries.GetOrCreateContact;

/// <summary>
/// Query to get an existing contact by email or create a new one
/// </summary>
public record GetOrCreateContactQuery : IRequest<Contact>
{
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? TimeZone { get; init; }
    public Guid UserId { get; init; }
    public ContactSource Source { get; init; } = ContactSource.Booking;
}
