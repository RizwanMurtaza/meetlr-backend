using Meetlr.Module.Homepage.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Module.Homepage.Infrastructure.Configurations;

public class UserHomepageConfiguration : IEntityTypeConfiguration<UserHomepage>
{
    public void Configure(EntityTypeBuilder<UserHomepage> builder)
    {
        builder.ToTable("UserHomepages");

        builder.HasKey(uh => uh.Id);

        // URL Settings
        builder.Property(uh => uh.Username)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(uh => uh.CustomDomain)
            .HasMaxLength(200);

        // SEO & Meta
        builder.Property(uh => uh.MetaTitle)
            .HasMaxLength(200);

        builder.Property(uh => uh.MetaDescription)
            .HasMaxLength(500);

        builder.Property(uh => uh.OgImageUrl)
            .HasMaxLength(500);

        // Branding
        builder.Property(uh => uh.PrimaryColor)
            .HasMaxLength(20);

        builder.Property(uh => uh.SecondaryColor)
            .HasMaxLength(20);

        builder.Property(uh => uh.AccentColor)
            .HasMaxLength(20);

        builder.Property(uh => uh.BackgroundColor)
            .HasMaxLength(20);

        builder.Property(uh => uh.TextColor)
            .HasMaxLength(20);

        builder.Property(uh => uh.FontFamily)
            .HasMaxLength(100);

        builder.Property(uh => uh.LogoUrl)
            .HasMaxLength(500);

        builder.Property(uh => uh.FaviconUrl)
            .HasMaxLength(500);

        // Hero Section
        builder.Property(uh => uh.HeroTitle)
            .HasMaxLength(500);

        builder.Property(uh => uh.HeroSubtitle)
            .HasMaxLength(1000);

        builder.Property(uh => uh.HeroImageUrl)
            .HasMaxLength(500);

        builder.Property(uh => uh.HeroBackgroundImageUrl)
            .HasMaxLength(500);

        builder.Property(uh => uh.HeroCtaText)
            .HasMaxLength(100);

        builder.Property(uh => uh.HeroCtaLink)
            .HasMaxLength(500);

        // About Section
        builder.Property(uh => uh.AboutTitle)
            .HasMaxLength(500);

        builder.Property(uh => uh.AboutContent)
            .HasColumnType("text");

        builder.Property(uh => uh.AboutImageUrl)
            .HasMaxLength(500);

        // Services Section
        builder.Property(uh => uh.ServicesJson)
            .HasColumnType("json");

        builder.Property(uh => uh.ServicesSectionTitle)
            .HasMaxLength(200);

        // Testimonials Section
        builder.Property(uh => uh.TestimonialsJson)
            .HasColumnType("json");

        builder.Property(uh => uh.TestimonialsSectionTitle)
            .HasMaxLength(200);

        // Gallery Section
        builder.Property(uh => uh.GalleryJson)
            .HasColumnType("json");

        builder.Property(uh => uh.GallerySectionTitle)
            .HasMaxLength(200);

        // Events Section
        builder.Property(uh => uh.EventsDisplayMode)
            .HasMaxLength(20)
            .HasDefaultValue("auto");

        builder.Property(uh => uh.SelectedEventIdsJson)
            .HasColumnType("json");

        builder.Property(uh => uh.MaxEventsToShow)
            .HasDefaultValue(6);

        builder.Property(uh => uh.EventsSectionTitle)
            .HasMaxLength(200);

        // Contact Section
        builder.Property(uh => uh.ContactEmail)
            .HasMaxLength(256);

        builder.Property(uh => uh.ContactPhone)
            .HasMaxLength(50);

        builder.Property(uh => uh.ContactAddress)
            .HasMaxLength(500);

        builder.Property(uh => uh.ContactFormEnabled)
            .HasMaxLength(10);

        // Social Links
        builder.Property(uh => uh.SocialLinksJson)
            .HasColumnType("json");

        // Section Configuration
        builder.Property(uh => uh.EnabledSectionsJson)
            .HasColumnType("json");

        // Custom CSS
        builder.Property(uh => uh.CustomCss)
            .HasColumnType("text");

        // Analytics
        builder.Property(uh => uh.ViewCount)
            .HasDefaultValue(0);

        // Indexes
        builder.HasIndex(uh => uh.UserId)
            .IsUnique()
            .HasDatabaseName("IX_UserHomepages_UserId"); // One homepage per user

        builder.HasIndex(uh => uh.Username)
            .IsUnique()
            .HasDatabaseName("IX_UserHomepages_Username");

        builder.HasIndex(uh => uh.CustomDomain)
            .IsUnique()
            .HasDatabaseName("IX_UserHomepages_CustomDomain");

        builder.HasIndex(uh => uh.TemplateId)
            .HasDatabaseName("IX_UserHomepages_TemplateId");

        builder.HasIndex(uh => uh.IsPublished)
            .HasDatabaseName("IX_UserHomepages_IsPublished");

        // Relationships
        builder.HasOne(uh => uh.User)
            .WithOne()
            .HasForeignKey<UserHomepage>(uh => uh.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(uh => uh.Template)
            .WithMany(t => t.UserHomepages)
            .HasForeignKey(uh => uh.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
