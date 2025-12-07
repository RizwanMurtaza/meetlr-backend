using System.Text.Json;
using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Module.Homepage.Domain.Entities;
using Meetlr.Domain.Entities.Users;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.Homepage.Application.Commands.SaveUserHomepage;

public class SaveUserHomepageCommandHandler : IRequestHandler<SaveUserHomepageCommand, SaveUserHomepageResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public SaveUserHomepageCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SaveUserHomepageResponse> Handle(
        SaveUserHomepageCommand request,
        CancellationToken cancellationToken)
    {
        // Validate user exists
        var user = await _unitOfWork.Repository<User>()
            .GetQueryable()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw UserErrors.UserNotFound(request.UserId);
        }

        // Validate template exists
        var template = await _unitOfWork.Repository<HomepageTemplate>()
            .GetQueryable()
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId && t.IsActive, cancellationToken);

        if (template == null)
        {
            throw new InvalidOperationException($"Template with ID {request.TemplateId} not found or is not active");
        }

        // Check if username is already taken (by another user)
        var usernameExists = await _unitOfWork.Repository<UserHomepage>()
            .GetQueryable()
            .AnyAsync(h => h.Username == request.Username.ToLower() && h.UserId != request.UserId, cancellationToken);

        if (usernameExists)
        {
            throw new InvalidOperationException($"Username '{request.Username}' is already taken");
        }

        // Get existing homepage or create new
        var homepage = await _unitOfWork.Repository<UserHomepage>()
            .GetQueryable()
            .FirstOrDefaultAsync(h => h.UserId == request.UserId, cancellationToken);

        var isNew = homepage == null;

        if (isNew)
        {
            homepage = new UserHomepage
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow
            };
        }

        // Update all fields
        homepage!.TemplateId = request.TemplateId;
        homepage.Username = request.Username.ToLower();
        homepage.CustomDomain = request.CustomDomain?.ToLower();

        if (request.IsPublished.HasValue)
        {
            var wasPublished = homepage.IsPublished;
            homepage.IsPublished = request.IsPublished.Value;

            // Set PublishedAt when first publishing
            if (request.IsPublished.Value && !wasPublished)
            {
                homepage.PublishedAt = DateTime.UtcNow;
            }
        }

        // SEO & Meta
        homepage.MetaTitle = request.MetaTitle;
        homepage.MetaDescription = request.MetaDescription;
        homepage.OgImageUrl = request.OgImageUrl;

        // Branding
        homepage.PrimaryColor = request.PrimaryColor;
        homepage.SecondaryColor = request.SecondaryColor;
        homepage.AccentColor = request.AccentColor;
        homepage.BackgroundColor = request.BackgroundColor;
        homepage.TextColor = request.TextColor;
        homepage.FontFamily = request.FontFamily;
        homepage.LogoUrl = request.LogoUrl;
        homepage.FaviconUrl = request.FaviconUrl;

        // Hero Section
        homepage.HeroTitle = request.HeroTitle;
        homepage.HeroSubtitle = request.HeroSubtitle;
        homepage.HeroImageUrl = request.HeroImageUrl;
        homepage.HeroBackgroundImageUrl = request.HeroBackgroundImageUrl;
        homepage.HeroCtaText = request.HeroCtaText;
        homepage.HeroCtaLink = request.HeroCtaLink;

        // About Section
        homepage.AboutTitle = request.AboutTitle;
        homepage.AboutContent = request.AboutContent;
        homepage.AboutImageUrl = request.AboutImageUrl;

        // Services Section
        homepage.ServicesJson = request.Services != null
            ? JsonSerializer.Serialize(request.Services)
            : null;
        homepage.ServicesSectionTitle = request.ServicesSectionTitle;

        // Testimonials Section
        homepage.TestimonialsJson = request.Testimonials != null
            ? JsonSerializer.Serialize(request.Testimonials)
            : null;
        homepage.TestimonialsSectionTitle = request.TestimonialsSectionTitle;

        // Gallery Section
        homepage.GalleryJson = request.Gallery != null
            ? JsonSerializer.Serialize(request.Gallery)
            : null;
        homepage.GallerySectionTitle = request.GallerySectionTitle;

        // Events Section
        if (request.ShowEvents.HasValue)
            homepage.ShowEvents = request.ShowEvents.Value;
        if (!string.IsNullOrEmpty(request.EventsDisplayMode))
            homepage.EventsDisplayMode = request.EventsDisplayMode;
        homepage.SelectedEventIdsJson = request.SelectedEventIds != null
            ? JsonSerializer.Serialize(request.SelectedEventIds)
            : null;
        if (request.MaxEventsToShow.HasValue)
            homepage.MaxEventsToShow = request.MaxEventsToShow.Value;
        homepage.EventsSectionTitle = request.EventsSectionTitle;

        // Contact Section
        homepage.ContactEmail = request.ContactEmail;
        homepage.ContactPhone = request.ContactPhone;
        homepage.ContactAddress = request.ContactAddress;
        homepage.ContactFormEnabled = request.ContactFormEnabled?.ToString().ToLower();

        // Social Links
        homepage.SocialLinksJson = request.SocialLinks != null
            ? JsonSerializer.Serialize(request.SocialLinks)
            : null;

        // Enabled Sections
        homepage.EnabledSectionsJson = request.EnabledSections != null
            ? JsonSerializer.Serialize(request.EnabledSections)
            : "[]";

        // Custom CSS
        homepage.CustomCss = request.CustomCss;

        homepage.UpdatedAt = DateTime.UtcNow;

        if (isNew)
        {
            _unitOfWork.Repository<UserHomepage>().Add(homepage);
        }
        else
        {
            _unitOfWork.Repository<UserHomepage>().Update(homepage);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SaveUserHomepageResponse
        {
            HomepageId = homepage.Id,
            Username = homepage.Username,
            IsPublished = homepage.IsPublished,
            Message = isNew ? "Homepage created successfully" : "Homepage updated successfully",
            PublicUrl = $"meetlr.com/{homepage.Username}",
            SubdomainUrl = $"{homepage.Username}.meetlr.com"
        };
    }
}
