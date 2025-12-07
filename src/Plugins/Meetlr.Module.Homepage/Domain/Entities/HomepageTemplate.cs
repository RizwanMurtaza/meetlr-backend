using Meetlr.Domain.Common;

namespace Meetlr.Module.Homepage.Domain.Entities;

/// <summary>
/// Represents a template design that users can choose for their public homepage.
/// Templates define the overall look and available sections.
/// This is a GLOBAL entity - not tenant-scoped, as templates are shared across all tenants.
/// </summary>
public class HomepageTemplate : BaseGlobalAuditableEntity
{
    // Basic Information
    public string Name { get; set; } = string.Empty; // e.g., "Professional Accountant"
    public string Slug { get; set; } = string.Empty; // e.g., "professional-accountant"
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // e.g., "Business", "Creative", "Minimal"

    // Preview
    public string PreviewImageUrl { get; set; } = string.Empty;
    public string? PreviewImageUrlMobile { get; set; }

    // Template Design (stored as Vue component name)
    public string ComponentName { get; set; } = string.Empty; // e.g., "HomepageTemplateAccountant"

    // Default Styling
    public string DefaultPrimaryColor { get; set; } = "#3B82F6";
    public string DefaultSecondaryColor { get; set; } = "#10B981";
    public string DefaultAccentColor { get; set; } = "#F59E0B";
    public string DefaultBackgroundColor { get; set; } = "#FFFFFF";
    public string DefaultTextColor { get; set; } = "#1F2937";
    public string DefaultFontFamily { get; set; } = "Inter";

    // Available Sections for this template (JSON array of section definitions)
    // e.g., [{"id": "hero", "name": "Hero Section", "required": true}, {"id": "services", "name": "Services", "required": false}]
    public string AvailableSectionsJson { get; set; } = "[]";

    // Default enabled sections when user selects this template (JSON array of section IDs)
    // e.g., ["hero", "about", "services", "events", "contact"]
    public string DefaultEnabledSectionsJson { get; set; } = "[]";

    // Status
    public bool IsActive { get; set; } = true;
    public bool IsFree { get; set; } = true; // For future premium templates
    public int DisplayOrder { get; set; } = 0;

    // Navigation properties
    public ICollection<UserHomepage> UserHomepages { get; set; } = new List<UserHomepage>();
}
