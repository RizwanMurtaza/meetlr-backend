using System.Text.Json;
using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Events;
using Meetlr.Module.Homepage.Domain.Entities;
using Meetlr.Module.Homepage.Application.Queries.GetUserHomepage;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.Homepage.Application.Queries.GetPublicHomepage;

public class GetPublicHomepageQueryHandler : IRequestHandler<GetPublicHomepageQuery, GetPublicHomepageResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPublicHomepageQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetPublicHomepageResponse> Handle(
        GetPublicHomepageQuery request,
        CancellationToken cancellationToken)
    {
        var username = request.Username.ToLower();

        // Get the homepage with template and user info
        var homepage = await _unitOfWork.Repository<UserHomepage>()
            .GetQueryable()
            .Include(h => h.Template)
            .Include(h => h.User)
            .FirstOrDefaultAsync(h => h.Username == username && h.IsPublished, cancellationToken);

        if (homepage == null || homepage.User == null)
        {
            return new GetPublicHomepageResponse
            {
                Found = false,
                Homepage = null
            };
        }

        // Increment view count
        homepage.ViewCount++;
        _unitOfWork.Repository<UserHomepage>().Update(homepage);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Get user's events if enabled
        List<PublicEventDto>? events = null;
        if (homepage.ShowEvents)
        {
            var eventsQuery = _unitOfWork.Repository<MeetlrEvent>()
                .GetQueryable()
                .Where(e => e.UserId == homepage.UserId && e.IsActive);

            // If manual mode, filter by selected event IDs
            if (homepage.EventsDisplayMode == "manual" && !string.IsNullOrEmpty(homepage.SelectedEventIdsJson))
            {
                var selectedIds = ParseJson<List<Guid>>(homepage.SelectedEventIdsJson);
                if (selectedIds != null && selectedIds.Any())
                {
                    eventsQuery = eventsQuery.Where(e => selectedIds.Contains(e.Id));
                }
            }

            events = await eventsQuery
                .Take(homepage.MaxEventsToShow)
                .Select(e => new PublicEventDto
                {
                    Id = e.Id,
                    Title = e.Name,
                    Description = e.Description,
                    Slug = e.Slug,
                    Duration = e.DurationMinutes,
                    Color = e.Color,
                    Price = e.Fee,
                    Currency = e.Currency,
                    BookingUrl = $"/book/{homepage.Username}/{e.Slug}"
                })
                .ToListAsync(cancellationToken);
        }

        var dto = new PublicHomepageDto
        {
            OwnerName = $"{homepage.User.FirstName} {homepage.User.LastName}".Trim(),
            OwnerProfileImageUrl = homepage.User.ProfileImageUrl,
            Username = homepage.Username,
            TemplateComponentName = homepage.Template?.ComponentName ?? string.Empty,
            TemplateSlug = homepage.Template?.Slug ?? string.Empty,
            MetaTitle = homepage.MetaTitle ?? $"{homepage.User.FirstName} {homepage.User.LastName}",
            MetaDescription = homepage.MetaDescription,
            OgImageUrl = homepage.OgImageUrl,
            PrimaryColor = homepage.PrimaryColor ?? homepage.Template?.DefaultPrimaryColor,
            SecondaryColor = homepage.SecondaryColor ?? homepage.Template?.DefaultSecondaryColor,
            AccentColor = homepage.AccentColor ?? homepage.Template?.DefaultAccentColor,
            BackgroundColor = homepage.BackgroundColor ?? homepage.Template?.DefaultBackgroundColor,
            TextColor = homepage.TextColor ?? homepage.Template?.DefaultTextColor,
            FontFamily = homepage.FontFamily ?? homepage.Template?.DefaultFontFamily,
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
            EventsSectionTitle = homepage.EventsSectionTitle,
            Events = events,
            ContactEmail = homepage.ContactEmail,
            ContactPhone = homepage.ContactPhone,
            ContactAddress = homepage.ContactAddress,
            ContactFormEnabled = homepage.ContactFormEnabled == "true",
            SocialLinks = ParseJson<SocialLinksDto>(homepage.SocialLinksJson),
            EnabledSections = ParseJson<List<string>>(homepage.EnabledSectionsJson) ?? new List<string>(),
            CustomCss = homepage.CustomCss
        };

        return new GetPublicHomepageResponse
        {
            Found = true,
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
