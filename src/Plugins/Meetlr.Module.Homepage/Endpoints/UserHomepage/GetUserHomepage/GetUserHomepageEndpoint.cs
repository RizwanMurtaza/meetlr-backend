using MediatR;
using Meetlr.Module.Homepage.Application.Queries.GetUserHomepage;
using Meetlr.Module.Homepage.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Homepage.Endpoints.UserHomepage.GetUserHomepage;

[Route("api/homepage")]
public class GetUserHomepageEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public GetUserHomepageEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get the current user's homepage configuration
    /// </summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(GetUserHomepageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle()
    {
        var query = new GetUserHomepageQuery
        {
            UserId = CurrentUserId
        };

        var queryResponse = await _mediator.Send(query);

        if (!queryResponse.HasHomepage || queryResponse.Homepage == null)
        {
            return Ok(new GetUserHomepageResponse
            {
                HasHomepage = false,
                Homepage = null
            });
        }

        var hp = queryResponse.Homepage;
        var response = new GetUserHomepageResponse
        {
            HasHomepage = true,
            Homepage = new UserHomepageData
            {
                Id = hp.Id,
                UserId = hp.UserId,
                TemplateId = hp.TemplateId,
                TemplateName = hp.TemplateName,
                TemplateSlug = hp.TemplateSlug,
                TemplateComponentName = hp.TemplateComponentName,
                Username = hp.Username,
                CustomDomain = hp.CustomDomain,
                IsPublished = hp.IsPublished,
                PublishedAt = hp.PublishedAt,
                MetaTitle = hp.MetaTitle,
                MetaDescription = hp.MetaDescription,
                OgImageUrl = hp.OgImageUrl,
                PrimaryColor = hp.PrimaryColor,
                SecondaryColor = hp.SecondaryColor,
                AccentColor = hp.AccentColor,
                BackgroundColor = hp.BackgroundColor,
                TextColor = hp.TextColor,
                FontFamily = hp.FontFamily,
                LogoUrl = hp.LogoUrl,
                FaviconUrl = hp.FaviconUrl,
                HeroTitle = hp.HeroTitle,
                HeroSubtitle = hp.HeroSubtitle,
                HeroImageUrl = hp.HeroImageUrl,
                HeroBackgroundImageUrl = hp.HeroBackgroundImageUrl,
                HeroCtaText = hp.HeroCtaText,
                HeroCtaLink = hp.HeroCtaLink,
                AboutTitle = hp.AboutTitle,
                AboutContent = hp.AboutContent,
                AboutImageUrl = hp.AboutImageUrl,
                Services = hp.Services?.Select(s => new ServiceItemResponse
                {
                    Title = s.Title,
                    Description = s.Description,
                    Icon = s.Icon,
                    Link = s.Link
                }).ToList(),
                ServicesSectionTitle = hp.ServicesSectionTitle,
                Testimonials = hp.Testimonials?.Select(t => new TestimonialItemResponse
                {
                    Name = t.Name,
                    Quote = t.Quote,
                    Company = t.Company,
                    ImageUrl = t.ImageUrl,
                    Rating = t.Rating
                }).ToList(),
                TestimonialsSectionTitle = hp.TestimonialsSectionTitle,
                Gallery = hp.Gallery?.Select(g => new GalleryItemResponse
                {
                    ImageUrl = g.ImageUrl,
                    Title = g.Title,
                    Description = g.Description
                }).ToList(),
                GallerySectionTitle = hp.GallerySectionTitle,
                ShowEvents = hp.ShowEvents,
                EventsDisplayMode = hp.EventsDisplayMode,
                SelectedEventIds = hp.SelectedEventIds,
                MaxEventsToShow = hp.MaxEventsToShow,
                EventsSectionTitle = hp.EventsSectionTitle,
                ContactEmail = hp.ContactEmail,
                ContactPhone = hp.ContactPhone,
                ContactAddress = hp.ContactAddress,
                ContactFormEnabled = hp.ContactFormEnabled,
                SocialLinks = hp.SocialLinks != null ? new SocialLinksResponse
                {
                    LinkedIn = hp.SocialLinks.LinkedIn,
                    Twitter = hp.SocialLinks.Twitter,
                    Instagram = hp.SocialLinks.Instagram,
                    Facebook = hp.SocialLinks.Facebook,
                    YouTube = hp.SocialLinks.YouTube,
                    TikTok = hp.SocialLinks.TikTok,
                    Website = hp.SocialLinks.Website
                } : null,
                EnabledSections = hp.EnabledSections,
                CustomCss = hp.CustomCss,
                ViewCount = hp.ViewCount
            }
        };

        return Ok(response);
    }
}
