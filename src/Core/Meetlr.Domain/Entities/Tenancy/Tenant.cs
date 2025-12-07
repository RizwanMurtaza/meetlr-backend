using Meetlr.Domain.Common;
using Meetlr.Domain.Entities.Users;

namespace Meetlr.Domain.Entities.Tenancy;

public class Tenant : BaseAuditableEntity
{
    // Basic Information
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty; // e.g., "tenant1" for tenant1.mywebsite.com
    public string? CustomDomain { get; set; } // e.g., user can point their domain to our website
    public string MainText { get; set; } = string.Empty; // Main headline/tagline for the tenant
    public string? Description { get; set; }

    // Contact Information
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }

    // Branding
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string? PrimaryColor { get; set; } = "#3B82F6"; // Default blue
    public string? SecondaryColor { get; set; } = "#10B981"; // Default green
    public string? AccentColor { get; set; } = "#F59E0B"; // Default amber
    public string? BackgroundColor { get; set; } = "#FFFFFF";
    public string? TextColor { get; set; } = "#1F2937";
    public string? FontFamily { get; set; } = "Inter, sans-serif";

    // SEO & Meta
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }

    // Settings
    public string TimeZone { get; set; } = "UTC";
    public string Language { get; set; } = "en";
    public string DateFormat { get; set; } = "MM/dd/yyyy";
    public string TimeFormat { get; set; } = "12h"; // 12h or 24h
    public string Currency { get; set; } = "USD";

    // Status & Subscription
    public bool IsActive { get; set; } = true;
    public bool IsSuperTenant { get; set; } = false; // For platform admin tenant
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public string? SubscriptionPlan { get; set; } // e.g., "Free", "Pro", "Enterprise"

    // Limits (for subscription tiers)
    public int MaxUsers { get; set; } = 10;
    public int MaxBookingsPerMonth { get; set; } = 100;
    public int MaxServices { get; set; } = 5;

    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Group> Groups { get; set; } = new List<Group>();
    public ICollection<Service> Services { get; set; } = new List<Service>();
}
