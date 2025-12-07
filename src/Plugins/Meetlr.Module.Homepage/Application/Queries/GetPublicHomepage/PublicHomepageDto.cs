using Meetlr.Module.Homepage.Application.Queries.GetUserHomepage;

namespace Meetlr.Module.Homepage.Application.Queries.GetPublicHomepage;

public record PublicHomepageDto
{
    // Owner Info
    public string OwnerName { get; init; } = string.Empty;
    public string? OwnerProfileImageUrl { get; init; }
    public string Username { get; init; } = string.Empty;

    // Template Info
    public string TemplateComponentName { get; init; } = string.Empty;
    public string TemplateSlug { get; init; } = string.Empty;

    // SEO & Meta
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public string? OgImageUrl { get; init; }

    // Branding/Styling
    public string? PrimaryColor { get; init; }
    public string? SecondaryColor { get; init; }
    public string? AccentColor { get; init; }
    public string? BackgroundColor { get; init; }
    public string? TextColor { get; init; }
    public string? FontFamily { get; init; }
    public string? LogoUrl { get; init; }
    public string? FaviconUrl { get; init; }

    // Hero Section
    public string? HeroTitle { get; init; }
    public string? HeroSubtitle { get; init; }
    public string? HeroImageUrl { get; init; }
    public string? HeroBackgroundImageUrl { get; init; }
    public string? HeroCtaText { get; init; }
    public string? HeroCtaLink { get; init; }

    // About Section
    public string? AboutTitle { get; init; }
    public string? AboutContent { get; init; }
    public string? AboutImageUrl { get; init; }

    // Services Section
    public List<ServiceItemDto>? Services { get; init; }
    public string? ServicesSectionTitle { get; init; }

    // Testimonials Section
    public List<TestimonialItemDto>? Testimonials { get; init; }
    public string? TestimonialsSectionTitle { get; init; }

    // Gallery Section
    public List<GalleryItemDto>? Gallery { get; init; }
    public string? GallerySectionTitle { get; init; }

    // Events (populated from user's events)
    public bool ShowEvents { get; init; }
    public string? EventsSectionTitle { get; init; }
    public List<PublicEventDto>? Events { get; init; }

    // Contact Section
    public string? ContactEmail { get; init; }
    public string? ContactPhone { get; init; }
    public string? ContactAddress { get; init; }
    public bool ContactFormEnabled { get; init; }

    // Social Links
    public SocialLinksDto? SocialLinks { get; init; }

    // Enabled Sections (for rendering)
    public List<string> EnabledSections { get; init; } = new();

    // Custom CSS
    public string? CustomCss { get; init; }
}

public record PublicEventDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Slug { get; init; } = string.Empty;
    public int Duration { get; init; }
    public string? Color { get; init; }
    public decimal? Price { get; init; }
    public string? Currency { get; init; }
    public string BookingUrl { get; init; } = string.Empty;
}
