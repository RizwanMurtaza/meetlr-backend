using Meetlr.Domain.Entities.Contacts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Infrastructure.Data.Configurations.Contacts;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("Contacts");

        builder.HasKey(c => c.Id);

        // Required fields
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(c => c.Phone)
            .HasMaxLength(50);

        builder.Property(c => c.TimeZone)
            .HasMaxLength(100);

        builder.Property(c => c.Company)
            .HasMaxLength(200);

        builder.Property(c => c.JobTitle)
            .HasMaxLength(100);

        builder.Property(c => c.ProfileImageUrl)
            .HasMaxLength(500);

        builder.Property(c => c.PreferredLanguage)
            .HasMaxLength(10);

        builder.Property(c => c.Tags)
            .HasMaxLength(500);

        builder.Property(c => c.BlockedReason)
            .HasMaxLength(500);

        builder.Property(c => c.CustomFieldsJson)
            .HasColumnType("text");

        builder.Property(c => c.Source)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(c => c.IsShared)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.IsBlacklisted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.MarketingConsent)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.TotalBookings)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(c => c.NoShowCount)
            .IsRequired()
            .HasDefaultValue(0);

        // ============================================
        // OPTIMIZED INDEX STRATEGY - 6 Strategic Indexes
        // ============================================

        // 1. UNIQUE CONSTRAINT INDEX - Tenant + Email uniqueness (one contact per email per tenant)
        // Supports: Contact creation validation, email lookups within tenant
        builder.HasIndex(c => new { c.TenantId, c.Email })
            .HasDatabaseName("IX_Contacts_TenantId_Email")
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        // 2. OWNER VIEW INDEX - Covers: user's contact list, sorting by last contacted
        // Supports: Contact list by owner, recent contacts
        builder.HasIndex(c => new { c.TenantId, c.UserId, c.LastContactedAt })
            .HasDatabaseName("IX_Contacts_TenantId_UserId_LastContacted");

        // 3. SHARED CONTACTS INDEX - Cross-tenant contact lookups
        // Supports: Shared contact queries
        builder.HasIndex(c => new { c.IsShared, c.Email })
            .HasDatabaseName("IX_Contacts_IsShared_Email")
            .HasFilter("[IsShared] = 1");

        // 4. BLACKLIST FILTER INDEX - Filter blacklisted contacts
        // Supports: Blacklist management, filtering blocked contacts
        builder.HasIndex(c => new { c.TenantId, c.IsBlacklisted, c.LastContactedAt })
            .HasDatabaseName("IX_Contacts_TenantId_Blacklisted_LastContacted")
            .HasFilter("[IsBlacklisted] = 1");

        // 5. EMAIL LOOKUP INDEX - Fast email searches across all tenants
        // Supports: Global email lookups, duplicate detection
        builder.HasIndex(c => c.Email)
            .HasDatabaseName("IX_Contacts_Email");

        // 6. CONTACT ACTIVITY INDEX - Sorting by contact frequency
        // Supports: Most/least contacted sorting, analytics
        builder.HasIndex(c => new { c.TenantId, c.TotalBookings, c.NoShowCount })
            .HasDatabaseName("IX_Contacts_TenantId_Activity");

        // Relationships
        builder.HasOne(c => c.Owner)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Bookings)
            .WithOne(b => b.Contact)
            .HasForeignKey(b => b.ContactId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
