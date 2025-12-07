using Meetlr.Module.Homepage.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Module.Homepage.Infrastructure.Configurations;

public class HomepageTemplateConfiguration : IEntityTypeConfiguration<HomepageTemplate>
{
    public void Configure(EntityTypeBuilder<HomepageTemplate> builder)
    {
        builder.ToTable("HomepageTemplates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.Property(t => t.Category)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.PreviewImageUrl)
            .HasMaxLength(500);

        builder.Property(t => t.PreviewImageUrlMobile)
            .HasMaxLength(500);

        builder.Property(t => t.ComponentName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.DefaultPrimaryColor)
            .HasMaxLength(20)
            .HasDefaultValue("#3B82F6");

        builder.Property(t => t.DefaultSecondaryColor)
            .HasMaxLength(20)
            .HasDefaultValue("#10B981");

        builder.Property(t => t.DefaultAccentColor)
            .HasMaxLength(20)
            .HasDefaultValue("#F59E0B");

        builder.Property(t => t.DefaultBackgroundColor)
            .HasMaxLength(20)
            .HasDefaultValue("#FFFFFF");

        builder.Property(t => t.DefaultTextColor)
            .HasMaxLength(20)
            .HasDefaultValue("#1F2937");

        builder.Property(t => t.DefaultFontFamily)
            .HasMaxLength(100)
            .HasDefaultValue("Inter");

        builder.Property(t => t.AvailableSectionsJson)
            .HasColumnType("json");

        builder.Property(t => t.DefaultEnabledSectionsJson)
            .HasColumnType("json");

        builder.Property(t => t.IsActive)
            .HasDefaultValue(true);

        builder.Property(t => t.IsFree)
            .HasDefaultValue(true);

        builder.Property(t => t.DisplayOrder)
            .HasDefaultValue(0);

        // Indexes
        builder.HasIndex(t => t.Slug)
            .IsUnique()
            .HasDatabaseName("IX_HomepageTemplates_Slug");

        builder.HasIndex(t => t.IsActive)
            .HasDatabaseName("IX_HomepageTemplates_IsActive");

        builder.HasIndex(t => new { t.IsActive, t.DisplayOrder })
            .HasDatabaseName("IX_HomepageTemplates_IsActive_DisplayOrder");

        builder.HasIndex(t => t.Category)
            .HasDatabaseName("IX_HomepageTemplates_Category");

        // Relationships
        builder.HasMany(t => t.UserHomepages)
            .WithOne(uh => uh.Template)
            .HasForeignKey(uh => uh.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
