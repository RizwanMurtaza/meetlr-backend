using Meetlr.Application.Common.Interfaces;
using Meetlr.Module.Homepage.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Module.Homepage.Infrastructure.Seeding;

/// <summary>
/// Seeds the default homepage templates for users to choose from
/// </summary>
public class HomepageTemplateSeeder : ISeeder
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<HomepageTemplateSeeder> _logger;

    public int Order => 1; // Run early before other seeders

    public HomepageTemplateSeeder(
        IUnitOfWork unitOfWork,
        ILogger<HomepageTemplateSeeder> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting homepage template seeding...");

        try
        {
            // Check if templates already exist
            var existingTemplates = await _unitOfWork.Repository<HomepageTemplate>()
                .GetQueryable()
                .IgnoreQueryFilters()
                .AnyAsync(cancellationToken);

            if (existingTemplates)
            {
                _logger.LogInformation("Homepage templates already exist, skipping seeding");
                return;
            }

            var templates = GetDefaultTemplates();

            foreach (var template in templates)
            {
                _unitOfWork.Repository<HomepageTemplate>().Add(template);
                _logger.LogInformation("Created homepage template: {TemplateName}", template.Name);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully seeded {Count} homepage templates", templates.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding homepage templates");
            throw;
        }
    }

    private static List<HomepageTemplate> GetDefaultTemplates()
    {
        var sectionsJson = GetAvailableSections();
        var defaultEnabledSections = "[\"hero\",\"about\",\"services\",\"events\",\"testimonials\",\"contact\"]";

        return new List<HomepageTemplate>
        {
            // 1. Professional Accountant Template
            new HomepageTemplate
            {
                Id = Guid.NewGuid(),
                Name = "Professional Accountant",
                Slug = "professional-accountant",
                Description = "Clean and trustworthy design perfect for accountants, financial advisors, and tax professionals. Features a professional color scheme and clear service presentation.",
                Category = "Business",
                PreviewImageUrl = "/templates/previews/professional-accountant.png",
                ComponentName = "HomepageTemplateProfessionalAccountant",
                DefaultPrimaryColor = "#1E3A5F",
                DefaultSecondaryColor = "#2ECC71",
                DefaultAccentColor = "#F39C12",
                DefaultBackgroundColor = "#FFFFFF",
                DefaultTextColor = "#2C3E50",
                DefaultFontFamily = "Inter",
                AvailableSectionsJson = sectionsJson,
                DefaultEnabledSectionsJson = defaultEnabledSections,
                IsActive = true,
                IsFree = true,
                DisplayOrder = 1,
                CreatedAt = DateTime.UtcNow
            },

            // 2. Tech Consultant Template
            new HomepageTemplate
            {
                Id = Guid.NewGuid(),
                Name = "Tech Consultant",
                Slug = "tech-consultant",
                Description = "Modern and sleek design for tech consultants, IT professionals, and software developers. Features a dark mode option and tech-focused aesthetics.",
                Category = "Technology",
                PreviewImageUrl = "/templates/previews/tech-consultant.png",
                ComponentName = "HomepageTemplateTechConsultant",
                DefaultPrimaryColor = "#6366F1",
                DefaultSecondaryColor = "#22D3EE",
                DefaultAccentColor = "#F472B6",
                DefaultBackgroundColor = "#0F172A",
                DefaultTextColor = "#E2E8F0",
                DefaultFontFamily = "JetBrains Mono",
                AvailableSectionsJson = sectionsJson,
                DefaultEnabledSectionsJson = defaultEnabledSections,
                IsActive = true,
                IsFree = true,
                DisplayOrder = 2,
                CreatedAt = DateTime.UtcNow
            },

            // 3. Life Coach Template
            new HomepageTemplate
            {
                Id = Guid.NewGuid(),
                Name = "Life Coach",
                Slug = "life-coach",
                Description = "Warm and inviting design for life coaches, wellness consultants, and personal development professionals. Features calming colors and inspirational imagery.",
                Category = "Wellness",
                PreviewImageUrl = "/templates/previews/life-coach.png",
                ComponentName = "HomepageTemplateLifeCoach",
                DefaultPrimaryColor = "#8B5CF6",
                DefaultSecondaryColor = "#EC4899",
                DefaultAccentColor = "#F59E0B",
                DefaultBackgroundColor = "#FDF2F8",
                DefaultTextColor = "#4C1D95",
                DefaultFontFamily = "Playfair Display",
                AvailableSectionsJson = sectionsJson,
                DefaultEnabledSectionsJson = defaultEnabledSections,
                IsActive = true,
                IsFree = true,
                DisplayOrder = 3,
                CreatedAt = DateTime.UtcNow
            },

            // 4. Lawyer Template
            new HomepageTemplate
            {
                Id = Guid.NewGuid(),
                Name = "Legal Professional",
                Slug = "legal-professional",
                Description = "Authoritative and professional design for lawyers, attorneys, and legal consultants. Features a traditional color scheme that conveys trust and expertise.",
                Category = "Business",
                PreviewImageUrl = "/templates/previews/legal-professional.png",
                ComponentName = "HomepageTemplateLegalProfessional",
                DefaultPrimaryColor = "#1F2937",
                DefaultSecondaryColor = "#B45309",
                DefaultAccentColor = "#DC2626",
                DefaultBackgroundColor = "#F9FAFB",
                DefaultTextColor = "#111827",
                DefaultFontFamily = "Merriweather",
                AvailableSectionsJson = sectionsJson,
                DefaultEnabledSectionsJson = defaultEnabledSections,
                IsActive = true,
                IsFree = true,
                DisplayOrder = 4,
                CreatedAt = DateTime.UtcNow
            },

            // 5. Freelancer Template
            new HomepageTemplate
            {
                Id = Guid.NewGuid(),
                Name = "Creative Freelancer",
                Slug = "creative-freelancer",
                Description = "Bold and creative design for freelancers, designers, and creative professionals. Features a portfolio-focused layout with vibrant colors.",
                Category = "Creative",
                PreviewImageUrl = "/templates/previews/creative-freelancer.png",
                ComponentName = "HomepageTemplateCreativeFreelancer",
                DefaultPrimaryColor = "#EF4444",
                DefaultSecondaryColor = "#3B82F6",
                DefaultAccentColor = "#10B981",
                DefaultBackgroundColor = "#FFFFFF",
                DefaultTextColor = "#1F2937",
                DefaultFontFamily = "Poppins",
                AvailableSectionsJson = sectionsJson,
                DefaultEnabledSectionsJson = "[\"hero\",\"about\",\"gallery\",\"services\",\"events\",\"testimonials\",\"contact\"]",
                IsActive = true,
                IsFree = true,
                DisplayOrder = 5,
                CreatedAt = DateTime.UtcNow
            },

            // 6. Creative Agency Template
            new HomepageTemplate
            {
                Id = Guid.NewGuid(),
                Name = "Creative Agency",
                Slug = "creative-agency",
                Description = "Dynamic and modern design for creative agencies, marketing firms, and design studios. Features an impactful hero section and portfolio showcase.",
                Category = "Creative",
                PreviewImageUrl = "/templates/previews/creative-agency.png",
                ComponentName = "HomepageTemplateCreativeAgency",
                DefaultPrimaryColor = "#7C3AED",
                DefaultSecondaryColor = "#06B6D4",
                DefaultAccentColor = "#FBBF24",
                DefaultBackgroundColor = "#000000",
                DefaultTextColor = "#FFFFFF",
                DefaultFontFamily = "Space Grotesk",
                AvailableSectionsJson = sectionsJson,
                DefaultEnabledSectionsJson = "[\"hero\",\"about\",\"gallery\",\"services\",\"events\",\"testimonials\",\"contact\"]",
                IsActive = true,
                IsFree = true,
                DisplayOrder = 6,
                CreatedAt = DateTime.UtcNow
            },

            // 7. Minimalist Template
            new HomepageTemplate
            {
                Id = Guid.NewGuid(),
                Name = "Minimalist",
                Slug = "minimalist",
                Description = "Clean and minimal design that works for any profession. Features a simple layout with focus on content and readability.",
                Category = "Minimal",
                PreviewImageUrl = "/templates/previews/minimalist.png",
                ComponentName = "HomepageTemplateMinimalist",
                DefaultPrimaryColor = "#000000",
                DefaultSecondaryColor = "#6B7280",
                DefaultAccentColor = "#3B82F6",
                DefaultBackgroundColor = "#FFFFFF",
                DefaultTextColor = "#1F2937",
                DefaultFontFamily = "Inter",
                AvailableSectionsJson = sectionsJson,
                DefaultEnabledSectionsJson = "[\"hero\",\"about\",\"services\",\"events\",\"contact\"]",
                IsActive = true,
                IsFree = true,
                DisplayOrder = 7,
                CreatedAt = DateTime.UtcNow
            }
        };
    }

    private static string GetAvailableSections()
    {
        return @"[
            {""id"":""hero"",""name"":""Hero Section"",""required"":true,""description"":""Main banner with your headline and call-to-action"",""icon"":""star""},
            {""id"":""about"",""name"":""About Me"",""required"":false,""description"":""Tell visitors about yourself and your expertise"",""icon"":""user""},
            {""id"":""services"",""name"":""Services"",""required"":false,""description"":""Showcase the services you offer"",""icon"":""briefcase""},
            {""id"":""events"",""name"":""Book Now"",""required"":false,""description"":""Display your available events for booking"",""icon"":""calendar""},
            {""id"":""testimonials"",""name"":""Testimonials"",""required"":false,""description"":""Show reviews and feedback from your clients"",""icon"":""message-circle""},
            {""id"":""gallery"",""name"":""Portfolio/Gallery"",""required"":false,""description"":""Display your work or portfolio images"",""icon"":""image""},
            {""id"":""contact"",""name"":""Contact"",""required"":false,""description"":""Your contact information and optional form"",""icon"":""mail""}
        ]";
    }
}
