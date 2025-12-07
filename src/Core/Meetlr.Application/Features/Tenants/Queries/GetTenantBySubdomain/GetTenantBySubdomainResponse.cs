namespace Meetlr.Application.Features.Tenants.Queries.GetTenantBySubdomain;

public record GetTenantBySubdomainResponse
{
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Subdomain { get; init; } = string.Empty;
    public string? CustomDomain { get; init; }
    public string MainText { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Website { get; init; }

    // Branding
    public string? LogoUrl { get; init; }
    public string? FaviconUrl { get; init; }
    public string? PrimaryColor { get; init; }
    public string? SecondaryColor { get; init; }
    public string? AccentColor { get; init; }
    public string? BackgroundColor { get; init; }
    public string? TextColor { get; init; }
    public string? FontFamily { get; init; }

    // SEO
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public string? MetaKeywords { get; init; }

    // Users (for listing or filtering by username)
    public List<TenantUserDto> Users { get; init; } = new();
}

