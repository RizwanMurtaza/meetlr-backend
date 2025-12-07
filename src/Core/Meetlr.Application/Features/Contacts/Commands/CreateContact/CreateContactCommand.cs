using Meetlr.Domain.Enums;
using MediatR;

namespace Meetlr.Application.Features.Contacts.Commands.CreateContact;

public record CreateContactCommand : IRequest<CreateContactCommandResponse>
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? TimeZone { get; init; }
    public string? Company { get; init; }
    public string? JobTitle { get; init; }
    public string? ProfileImageUrl { get; init; }
    public string? PreferredLanguage { get; init; }
    public string? Tags { get; init; }
    public bool MarketingConsent { get; init; }
    public ContactSource Source { get; init; } = ContactSource.Manual;
    public bool IsShared { get; init; } = false;
    public string? CustomFieldsJson { get; init; }
}
