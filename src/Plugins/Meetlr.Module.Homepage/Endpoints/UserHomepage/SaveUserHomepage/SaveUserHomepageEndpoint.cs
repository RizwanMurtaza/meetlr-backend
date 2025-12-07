using MediatR;
using Meetlr.Module.Homepage.Application.Commands.SaveUserHomepage;
using Meetlr.Module.Homepage.Application.Queries.GetUserHomepage;
using Meetlr.Module.Homepage.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Homepage.Endpoints.UserHomepage.SaveUserHomepage;

[Route("api/homepage")]
public class SaveUserHomepageEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public SaveUserHomepageEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Save/update the current user's homepage configuration
    /// </summary>
    [HttpPost("my")]
    [ProducesResponseType(typeof(SaveUserHomepageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle([FromBody] SaveUserHomepageRequest request)
    {
        var command = new SaveUserHomepageCommand
        {
            UserId = CurrentUserId,
            TemplateId = request.TemplateId,
            Username = request.Username,
            CustomDomain = request.CustomDomain,
            IsPublished = request.IsPublished,
            MetaTitle = request.MetaTitle,
            MetaDescription = request.MetaDescription,
            OgImageUrl = request.OgImageUrl,
            PrimaryColor = request.PrimaryColor,
            SecondaryColor = request.SecondaryColor,
            AccentColor = request.AccentColor,
            BackgroundColor = request.BackgroundColor,
            TextColor = request.TextColor,
            FontFamily = request.FontFamily,
            LogoUrl = request.LogoUrl,
            FaviconUrl = request.FaviconUrl,
            HeroTitle = request.HeroTitle,
            HeroSubtitle = request.HeroSubtitle,
            HeroImageUrl = request.HeroImageUrl,
            HeroBackgroundImageUrl = request.HeroBackgroundImageUrl,
            HeroCtaText = request.HeroCtaText,
            HeroCtaLink = request.HeroCtaLink,
            AboutTitle = request.AboutTitle,
            AboutContent = request.AboutContent,
            AboutImageUrl = request.AboutImageUrl,
            Services = request.Services?.Select(s => new ServiceItemDto
            {
                Title = s.Title,
                Description = s.Description,
                Icon = s.Icon,
                Link = s.Link
            }).ToList(),
            ServicesSectionTitle = request.ServicesSectionTitle,
            Testimonials = request.Testimonials?.Select(t => new TestimonialItemDto
            {
                Name = t.Name,
                Quote = t.Quote,
                Company = t.Company,
                ImageUrl = t.ImageUrl,
                Rating = t.Rating
            }).ToList(),
            TestimonialsSectionTitle = request.TestimonialsSectionTitle,
            Gallery = request.Gallery?.Select(g => new GalleryItemDto
            {
                ImageUrl = g.ImageUrl,
                Title = g.Title,
                Description = g.Description
            }).ToList(),
            GallerySectionTitle = request.GallerySectionTitle,
            ShowEvents = request.ShowEvents,
            EventsDisplayMode = request.EventsDisplayMode,
            SelectedEventIds = request.SelectedEventIds,
            MaxEventsToShow = request.MaxEventsToShow,
            EventsSectionTitle = request.EventsSectionTitle,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            ContactAddress = request.ContactAddress,
            ContactFormEnabled = request.ContactFormEnabled,
            SocialLinks = request.SocialLinks != null ? new SocialLinksDto
            {
                LinkedIn = request.SocialLinks.LinkedIn,
                Twitter = request.SocialLinks.Twitter,
                Instagram = request.SocialLinks.Instagram,
                Facebook = request.SocialLinks.Facebook,
                YouTube = request.SocialLinks.YouTube,
                TikTok = request.SocialLinks.TikTok,
                Website = request.SocialLinks.Website
            } : null,
            EnabledSections = request.EnabledSections,
            CustomCss = request.CustomCss
        };

        var commandResponse = await _mediator.Send(command);

        var response = new SaveUserHomepageResponse
        {
            HomepageId = commandResponse.HomepageId,
            Username = commandResponse.Username,
            IsPublished = commandResponse.IsPublished,
            Message = commandResponse.Message,
            PublicUrl = commandResponse.PublicUrl,
            SubdomainUrl = commandResponse.SubdomainUrl
        };

        return Ok(response);
    }
}
