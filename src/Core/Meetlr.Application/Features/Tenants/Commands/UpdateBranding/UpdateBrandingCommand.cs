using MediatR;

namespace Meetlr.Application.Features.Tenants.Commands.UpdateBranding;

/// <summary>
/// Command for updating tenant branding by admin users
/// </summary>
public record UpdateBrandingCommand : IRequest<UpdateBrandingResponse>
{
    public Guid TenantId { get; init; }
    public string? LogoUrl { get; init; }
    public string? FaviconUrl { get; init; }
    public string? PrimaryColor { get; init; }
    public string? SecondaryColor { get; init; }
    public string? AccentColor { get; init; }
    public string? BackgroundColor { get; init; }
    public string? TextColor { get; init; }
    public string? FontFamily { get; init; }
    public string? MainText { get; init; }
    public string? Description { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public string? MetaKeywords { get; init; }
}
