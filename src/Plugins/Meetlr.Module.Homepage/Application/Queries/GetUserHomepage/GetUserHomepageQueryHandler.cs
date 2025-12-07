using System.Text.Json;
using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Module.Homepage.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.Homepage.Application.Queries.GetUserHomepage;

public class GetUserHomepageQueryHandler : IRequestHandler<GetUserHomepageQuery, GetUserHomepageResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserHomepageQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetUserHomepageResponse> Handle(
        GetUserHomepageQuery request,
        CancellationToken cancellationToken)
    {
        var homepage = await _unitOfWork.Repository<UserHomepage>()
            .GetQueryable()
            .Include(h => h.Template)
            .FirstOrDefaultAsync(h => h.UserId == request.UserId, cancellationToken);

        if (homepage == null)
        {
            return new GetUserHomepageResponse
            {
                HasHomepage = false,
                Homepage = null
            };
        }

        var dto = new UserHomepageDto
        {
            Id = homepage.Id,
            UserId = homepage.UserId,
            TemplateId = homepage.TemplateId,
            TemplateName = homepage.Template?.Name ?? string.Empty,
            TemplateSlug = homepage.Template?.Slug ?? string.Empty,
            TemplateComponentName = homepage.Template?.ComponentName ?? string.Empty,
            Username = homepage.Username,
            CustomDomain = homepage.CustomDomain,
            IsPublished = homepage.IsPublished,
            PublishedAt = homepage.PublishedAt,
            MetaTitle = homepage.MetaTitle,
            MetaDescription = homepage.MetaDescription,
            OgImageUrl = homepage.OgImageUrl,
            PrimaryColor = homepage.PrimaryColor,
            SecondaryColor = homepage.SecondaryColor,
            AccentColor = homepage.AccentColor,
            BackgroundColor = homepage.BackgroundColor,
            TextColor = homepage.TextColor,
            FontFamily = homepage.FontFamily,
            LogoUrl = homepage.LogoUrl,
            FaviconUrl = homepage.FaviconUrl,
            HeroTitle = homepage.HeroTitle,
            HeroSubtitle = homepage.HeroSubtitle,
            HeroImageUrl = homepage.HeroImageUrl,
            HeroBackgroundImageUrl = homepage.HeroBackgroundImageUrl,
            HeroCtaText = homepage.HeroCtaText,
            HeroCtaLink = homepage.HeroCtaLink,
            AboutTitle = homepage.AboutTitle,
            AboutContent = homepage.AboutContent,
            AboutImageUrl = homepage.AboutImageUrl,
            Services = ParseJson<List<ServiceItemDto>>(homepage.ServicesJson),
            ServicesSectionTitle = homepage.ServicesSectionTitle,
            Testimonials = ParseJson<List<TestimonialItemDto>>(homepage.TestimonialsJson),
            TestimonialsSectionTitle = homepage.TestimonialsSectionTitle,
            Gallery = ParseJson<List<GalleryItemDto>>(homepage.GalleryJson),
            GallerySectionTitle = homepage.GallerySectionTitle,
            ShowEvents = homepage.ShowEvents,
            EventsDisplayMode = homepage.EventsDisplayMode,
            SelectedEventIds = ParseJson<List<Guid>>(homepage.SelectedEventIdsJson),
            MaxEventsToShow = homepage.MaxEventsToShow,
            EventsSectionTitle = homepage.EventsSectionTitle,
            ContactEmail = homepage.ContactEmail,
            ContactPhone = homepage.ContactPhone,
            ContactAddress = homepage.ContactAddress,
            ContactFormEnabled = homepage.ContactFormEnabled == "true",
            SocialLinks = ParseJson<SocialLinksDto>(homepage.SocialLinksJson),
            EnabledSections = ParseJson<List<string>>(homepage.EnabledSectionsJson) ?? new List<string>(),
            CustomCss = homepage.CustomCss,
            ViewCount = homepage.ViewCount
        };

        return new GetUserHomepageResponse
        {
            HasHomepage = true,
            Homepage = dto
        };
    }

    private static T? ParseJson<T>(string? json) where T : class
    {
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return null;
        }
    }
}
