using  Meetlr.Domain.Entities.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Infrastructure.Data.Configurations.Tenancy;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Subdomain)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.CustomDomain)
            .HasMaxLength(200);

        builder.Property(t => t.MainText)
            .HasMaxLength(500);

        builder.Property(t => t.Description)
            .HasMaxLength(2000);

        builder.Property(t => t.Email)
            .HasMaxLength(256);

        builder.Property(t => t.PhoneNumber)
            .HasMaxLength(50);

        builder.Property(t => t.Website)
            .HasMaxLength(500);

        builder.Property(t => t.Address)
            .HasMaxLength(500);

        builder.Property(t => t.City)
            .HasMaxLength(100);

        builder.Property(t => t.Country)
            .HasMaxLength(100);

        builder.Property(t => t.PostalCode)
            .HasMaxLength(20);

        builder.Property(t => t.LogoUrl)
            .HasMaxLength(500);

        builder.Property(t => t.FaviconUrl)
            .HasMaxLength(500);

        builder.Property(t => t.PrimaryColor)
            .HasMaxLength(20)
            .HasDefaultValue("#3B82F6");

        builder.Property(t => t.SecondaryColor)
            .HasMaxLength(20)
            .HasDefaultValue("#10B981");

        builder.Property(t => t.AccentColor)
            .HasMaxLength(20)
            .HasDefaultValue("#F59E0B");

        builder.Property(t => t.BackgroundColor)
            .HasMaxLength(20)
            .HasDefaultValue("#FFFFFF");

        builder.Property(t => t.TextColor)
            .HasMaxLength(20)
            .HasDefaultValue("#1F2937");

        builder.Property(t => t.FontFamily)
            .HasMaxLength(100)
            .HasDefaultValue("Inter, sans-serif");

        builder.Property(t => t.MetaTitle)
            .HasMaxLength(200);

        builder.Property(t => t.MetaDescription)
            .HasMaxLength(500);

        builder.Property(t => t.MetaKeywords)
            .HasMaxLength(500);

        builder.Property(t => t.TimeZone)
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue("UTC");

        builder.Property(t => t.Language)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("en");

        builder.Property(t => t.DateFormat)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("MM/dd/yyyy");

        builder.Property(t => t.TimeFormat)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("12h");

        builder.Property(t => t.Currency)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("USD");

        builder.Property(t => t.SubscriptionPlan)
            .HasMaxLength(50);

        builder.Property(t => t.MaxUsers)
            .HasDefaultValue(10);

        builder.Property(t => t.MaxBookingsPerMonth)
            .HasDefaultValue(100);

        builder.Property(t => t.MaxServices)
            .HasDefaultValue(5);

        // ============================================
        // OPTIMIZED INDEX STRATEGY - 5 Strategic Indexes
        // ============================================

        // 1. UNIQUE SUBDOMAIN INDEX - Subdomain routing
        // Supports: Tenant resolution by subdomain (app.meetlr.com)
        builder.HasIndex(t => t.Subdomain)
            .IsUnique()
            .HasDatabaseName("IX_Tenants_Subdomain");

        // 2. UNIQUE CUSTOM DOMAIN INDEX - Custom domain routing
        // Supports: Tenant resolution by custom domain (mycustom.com)
        builder.HasIndex(t => t.CustomDomain)
            .IsUnique()
            .HasFilter("[CustomDomain] IS NOT NULL")
            .HasDatabaseName("IX_Tenants_CustomDomain");

        // 3. ACTIVE TENANTS INDEX - Filter active tenants with creation date sorting
        // Supports: Admin dashboard, active tenant lists, analytics
        builder.HasIndex(t => new { t.IsActive, t.CreatedAt })
            .HasDatabaseName("IX_Tenants_IsActive_CreatedAt");

        // 4. SUPER TENANT INDEX - System admin queries
        // Supports: Super tenant identification
        builder.HasIndex(t => t.IsSuperTenant)
            .HasDatabaseName("IX_Tenants_IsSuperTenant")
            .HasFilter("[IsSuperTenant] = 1");

        // Relationships
        builder.HasMany(t => t.Users)
            .WithOne(u => u.Tenant)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Groups)
            .WithOne(g => g.Tenant)
            .HasForeignKey(g => g.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Services)
            .WithOne(s => s.Tenant)
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
