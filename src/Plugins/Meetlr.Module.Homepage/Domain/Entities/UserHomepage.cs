using Meetlr.Domain.Common;
using Meetlr.Domain.Entities.Users;

namespace Meetlr.Module.Homepage.Domain.Entities;

/// <summary>
/// Represents a user's customized homepage built from a template.
/// Contains all user-specific content and customizations.
/// </summary>
public class UserHomepage : BaseAuditableEntity
{
    // Ownership
    public Guid UserId { get; set; }
    public User? User { get; set; }

    // Template Reference
    public Guid TemplateId { get; set; }
    public HomepageTemplate? Template { get; set; }

    // URL Settings
    public string Username { get; set; } = string.Empty; // Used for meetlr.com/{username} and {username}.meetlr.com
    public string? CustomDomain { get; set; } // For future: user's own domain

    // Publishing Status
    public bool IsPublished { get; set; } = false;
    public DateTime? PublishedAt { get; set; }

    // SEO & Meta
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? OgImageUrl { get; set; }

    // Branding/Styling Customizations
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? AccentColor { get; set; }
    public string? BackgroundColor { get; set; }
    public string? TextColor { get; set; }
    public string? FontFamily { get; set; }
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }

    // Hero Section Content
    public string? HeroTitle { get; set; }
    public string? HeroSubtitle { get; set; }
    public string? HeroImageUrl { get; set; }
    public string? HeroBackgroundImageUrl { get; set; }
    public string? HeroCtaText { get; set; } // e.g., "Book a Consultation"
    public string? HeroCtaLink { get; set; } // Where the CTA button links to

    // About Section Content
    public string? AboutTitle { get; set; }
    public string? AboutContent { get; set; } // Rich text/HTML
    public string? AboutImageUrl { get; set; }

    // Services Section (JSON array)
    // e.g., [{"title": "Tax Consultation", "description": "...", "icon": "calculator", "link": "/book/tax"}]
    public string? ServicesJson { get; set; }
    public string? ServicesSectionTitle { get; set; }

    // Testimonials Section (JSON array)
    // e.g., [{"name": "John Doe", "quote": "Great service!", "company": "ABC Corp", "imageUrl": "..."}]
    public string? TestimonialsJson { get; set; }
    public string? TestimonialsSectionTitle { get; set; }

    // Gallery/Portfolio Section (JSON array)
    // e.g., [{"imageUrl": "...", "title": "Project A", "description": "..."}]
    public string? GalleryJson { get; set; }
    public string? GallerySectionTitle { get; set; }

    // Events Integration Settings
    public bool ShowEvents { get; set; } = true;
    public string EventsDisplayMode { get; set; } = "auto"; // "auto" = show all published, "manual" = selected only
    public string? SelectedEventIdsJson { get; set; } // JSON array of event IDs if manual mode
    public int MaxEventsToShow { get; set; } = 6;
    public string? EventsSectionTitle { get; set; }

    // Contact Section
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactAddress { get; set; }
    public string? ContactFormEnabled { get; set; } // "true" or "false"

    // Social Links (JSON object)
    // e.g., {"linkedin": "https://...", "twitter": "https://...", "instagram": "https://..."}
    public string? SocialLinksJson { get; set; }

    // Section Order and Visibility (JSON array)
    // e.g., ["hero", "about", "services", "events", "testimonials", "contact"]
    public string EnabledSectionsJson { get; set; } = "[]";

    // Custom CSS (advanced users)
    public string? CustomCss { get; set; }

    // Analytics
    public int ViewCount { get; set; } = 0;
}
