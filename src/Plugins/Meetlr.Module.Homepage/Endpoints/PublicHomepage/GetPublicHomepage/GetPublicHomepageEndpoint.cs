using MediatR;
using Meetlr.Module.Homepage.Application.Queries.GetPublicHomepage;
using Meetlr.Module.Homepage.Endpoints.UserHomepage.GetUserHomepage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Homepage.Endpoints.PublicHomepage.GetPublicHomepage;

[ApiController]
[Route("api/public/homepage")]
public class GetPublicHomepageEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public GetPublicHomepageEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get a public homepage by username (no auth required)
    /// </summary>
    [HttpGet("{username}")]
    [ProducesResponseType(typeof(GetPublicHomepageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle(string username)
    {
        var query = new GetPublicHomepageQuery
        {
            Username = username
        };

        var queryResponse = await _mediator.Send(query);

        if (!queryResponse.Found || queryResponse.Homepage == null)
        {
            return NotFound(new GetPublicHomepageResponse
            {
                Found = false,
                Homepage = null
            });
        }

        var hp = queryResponse.Homepage;
        var response = new GetPublicHomepageResponse
        {
            Found = true,
            Homepage = new PublicHomepageData
            {
                OwnerName = hp.OwnerName,
                OwnerProfileImageUrl = hp.OwnerProfileImageUrl,
                Username = hp.Username,
                TemplateComponentName = hp.TemplateComponentName,
                TemplateSlug = hp.TemplateSlug,
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
                EventsSectionTitle = hp.EventsSectionTitle,
                Events = hp.Events?.Select(e => new PublicEventResponse
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    Slug = e.Slug,
                    Duration = e.Duration,
                    Color = e.Color,
                    Price = e.Price,
                    Currency = e.Currency,
                    BookingUrl = e.BookingUrl
                }).ToList(),
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
                CustomCss = hp.CustomCss
            }
        };

        return Ok(response);
    }
}
