using MediatR;

namespace Meetlr.Application.Features.Contacts.Commands.UpdateContact;

public record UpdateContactCommand : IRequest<UpdateContactCommandResponse>
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? TimeZone { get; init; }
    public string? Company { get; init; }
    public string? JobTitle { get; init; }
    public string? ProfileImageUrl { get; init; }
    public string? PreferredLanguage { get; init; }
    public string? Tags { get; init; }
    public bool? MarketingConsent { get; init; }
    public bool? IsShared { get; init; }
    public bool? IsBlacklisted { get; init; }
    public string? BlockedReason { get; init; }
    public string? CustomFieldsJson { get; init; }
}
