using System.Text.Json.Serialization;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Entities.Payments;
using Meetlr.Domain.Entities.Scheduling;
using Meetlr.Domain.Entities.Tenancy;
using Microsoft.AspNetCore.Identity;

namespace Meetlr.Domain.Entities.Users;

/// <summary>
/// User entity that combines ASP.NET Identity with Calendly business properties
/// This replaces both the old User and ApplicationUser entities
/// Maps to "Users" table (not AspNetUsers)
/// </summary>
public class User : IdentityUser<Guid>
{
    // Business Properties
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    // Email inherited from IdentityUser
    // PhoneNumber inherited from IdentityUser
    public string? ProfileImageUrl { get; set; }
    public string? Bio { get; set; }
    public string? CompanyName { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public string? MeetlrUsername { get; set; } // For shareable links: tenant.mywebsite.com/{username}
    public string? WelcomeMessage { get; set; }
    public string? LogoUrl { get; set; }
    public string? BrandColor { get; set; }
    public string? Language { get; set; } = "en";
    public string? DateFormat { get; set; } = "MM/dd/yyyy";
    public string? TimeFormat { get; set; } = "12h";
    public bool UseBranding { get; set; } = true;

    // Identity & Status
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    // OAuth/External Login
    public string? OAuthProvider { get; set; } // "Google", "Microsoft", etc.
    public string? OAuthProviderId { get; set; } // Unique ID from OAuth provider

    // Tenant relationship
    public Guid TenantId { get; set; }

    // Audit fields (from BaseAuditableEntity pattern)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    // Navigation properties
    [JsonIgnore]
    public Tenant Tenant { get; set; } = null!;
    [JsonIgnore]
    public StripeAccount? StripeAccount { get; set; }
    [JsonIgnore]
    public ICollection<AvailabilitySchedule> AvailabilitySchedules { get; set; } = new List<AvailabilitySchedule>();
    [JsonIgnore]
    public ICollection<MeetlrEvent> MeetlrEvents { get; set; } = new List<MeetlrEvent>();
    [JsonIgnore]
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    [JsonIgnore]
    public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
    [JsonIgnore]
    public ICollection<Service> ProvidedServices { get; set; } = new List<Service>();
    [JsonIgnore]
    public UserSettings UserSettings { get; set; } = null!;
}
