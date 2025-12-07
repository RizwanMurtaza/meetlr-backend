using System.ComponentModel.DataAnnotations;

namespace Meetlr.Api.Models.Contacts;

public class CreateContactRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? TimeZone { get; set; }

    [MaxLength(200)]
    public string? Company { get; set; }

    [MaxLength(100)]
    public string? JobTitle { get; set; }

    [Url]
    [MaxLength(500)]
    public string? ProfileImageUrl { get; set; }

    [MaxLength(10)]
    public string? PreferredLanguage { get; set; }

    [MaxLength(500)]
    public string? Tags { get; set; }

    public bool MarketingConsent { get; set; }

    public string? Source { get; set; } = "Manual";

    public bool IsShared { get; set; } = false;

    public string? CustomFieldsJson { get; set; }
}
