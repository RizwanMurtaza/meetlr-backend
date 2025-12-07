using Meetlr.Module.Homepage.Application.Commands.SaveUserHomepage;
using Meetlr.Module.Homepage.Application.Queries.GetUserHomepage;

namespace Meetlr.Module.Homepage.Endpoints.Save;

public record SaveUserHomepageRequest
{
    public Guid TemplateId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string? CustomDomain { get; init; }
    public bool? IsPublished { get; init; }

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

    // Events Section
    public bool? ShowEvents { get; init; }
    public string? EventsDisplayMode { get; init; }
    public List<Guid>? SelectedEventIds { get; init; }
    public int? MaxEventsToShow { get; init; }
    public string? EventsSectionTitle { get; init; }

    // Contact Section
    public string? ContactEmail { get; init; }
    public string? ContactPhone { get; init; }
    public string? ContactAddress { get; init; }
    public bool? ContactFormEnabled { get; init; }

    // Social Links
    public SocialLinksDto? SocialLinks { get; init; }

    // Enabled Sections
    public List<string>? EnabledSections { get; init; }

    // Custom CSS
    public string? CustomCss { get; init; }

    public SaveUserHomepageCommand ToCommand(Guid userId)
    {
        return new SaveUserHomepageCommand
        {
            UserId = userId,
            TemplateId = TemplateId,
            Username = Username,
            CustomDomain = CustomDomain,
            IsPublished = IsPublished,
            MetaTitle = MetaTitle,
            MetaDescription = MetaDescription,
            OgImageUrl = OgImageUrl,
            PrimaryColor = PrimaryColor,
            SecondaryColor = SecondaryColor,
            AccentColor = AccentColor,
            BackgroundColor = BackgroundColor,
            TextColor = TextColor,
            FontFamily = FontFamily,
            LogoUrl = LogoUrl,
            FaviconUrl = FaviconUrl,
            HeroTitle = HeroTitle,
            HeroSubtitle = HeroSubtitle,
            HeroImageUrl = HeroImageUrl,
            HeroBackgroundImageUrl = HeroBackgroundImageUrl,
            HeroCtaText = HeroCtaText,
            HeroCtaLink = HeroCtaLink,
            AboutTitle = AboutTitle,
            AboutContent = AboutContent,
            AboutImageUrl = AboutImageUrl,
            Services = Services,
            ServicesSectionTitle = ServicesSectionTitle,
            Testimonials = Testimonials,
            TestimonialsSectionTitle = TestimonialsSectionTitle,
            Gallery = Gallery,
            GallerySectionTitle = GallerySectionTitle,
            ShowEvents = ShowEvents,
            EventsDisplayMode = EventsDisplayMode,
            SelectedEventIds = SelectedEventIds,
            MaxEventsToShow = MaxEventsToShow,
            EventsSectionTitle = EventsSectionTitle,
            ContactEmail = ContactEmail,
            ContactPhone = ContactPhone,
            ContactAddress = ContactAddress,
            ContactFormEnabled = ContactFormEnabled,
            SocialLinks = SocialLinks,
            EnabledSections = EnabledSections,
            CustomCss = CustomCss
        };
    }
}
