using Meetlr.Domain.Enums;

namespace Meetlr.Application.Features.Contacts.Queries.GetContactById;

public record ContactDetailDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? TimeZone { get; init; }
    public string? Company { get; init; }
    public string? JobTitle { get; init; }
    public string? ProfileImageUrl { get; init; }
    public string? PreferredLanguage { get; init; }
    public string? Tags { get; init; }
    public ContactSource Source { get; init; }
    public bool IsShared { get; init; }
    public bool IsBlacklisted { get; init; }
    public string? BlockedReason { get; init; }
    public bool MarketingConsent { get; init; }
    public int TotalBookings { get; init; }
    public int NoShowCount { get; init; }
    public DateTime? LastContactedAt { get; init; }
    public string? CustomFieldsJson { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
