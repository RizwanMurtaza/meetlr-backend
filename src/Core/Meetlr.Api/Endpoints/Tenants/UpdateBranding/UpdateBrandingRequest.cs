using Meetlr.Application.Features.Tenants.Commands.UpdateBranding;

namespace Meetlr.Api.Endpoints.Tenants.UpdateBranding;

public record UpdateBrandingRequest
{
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

    public static UpdateBrandingCommand ToCommand(UpdateBrandingRequest request, Guid tenantId)
    {
        return new UpdateBrandingCommand
        {
            TenantId = tenantId,
            LogoUrl = request.LogoUrl,
            FaviconUrl = request.FaviconUrl,
            PrimaryColor = request.PrimaryColor,
            SecondaryColor = request.SecondaryColor,
            AccentColor = request.AccentColor,
            BackgroundColor = request.BackgroundColor,
            TextColor = request.TextColor,
            FontFamily = request.FontFamily,
            MainText = request.MainText,
            Description = request.Description,
            MetaTitle = request.MetaTitle,
            MetaDescription = request.MetaDescription,
            MetaKeywords = request.MetaKeywords
        };
    }
}
