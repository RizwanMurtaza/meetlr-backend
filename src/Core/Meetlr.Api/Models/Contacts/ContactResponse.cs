namespace Meetlr.Api.Models.Contacts;

public class ContactResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? TimeZone { get; set; }
    public string? Company { get; set; }
    public string? JobTitle { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? Tags { get; set; }
    public string Source { get; set; } = string.Empty;
    public bool IsShared { get; set; }
    public bool IsBlacklisted { get; set; }
    public string? BlockedReason { get; set; }
    public bool MarketingConsent { get; set; }
    public int TotalBookings { get; set; }
    public int NoShowCount { get; set; }
    public DateTime? LastContactedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
