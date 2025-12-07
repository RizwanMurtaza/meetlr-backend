using  Meetlr.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Infrastructure.Data.Configurations.Users;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.MeetlrUsername)
            .HasMaxLength(100);

        builder.Property(u => u.TimeZone)
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue("UTC");

        // ============================================
        // OPTIMIZED INDEX STRATEGY - 5 Strategic Indexes
        // ============================================

        // 1. UNIQUE EMAIL INDEX - Global email uniqueness
        // Supports: User authentication, email lookups
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        // 2. UNIQUE USERNAME INDEX - Public profile URL uniqueness
        // Supports: Public booking pages (username/event-slug)
        builder.HasIndex(u => u.MeetlrUsername)
            .IsUnique()
            .HasFilter("[MeetlrUsername] IS NOT NULL")
            .HasDatabaseName("IX_Users_MeetlrUsername");

        // 3. TENANT USERS INDEX - Covers: tenant user list with soft delete filtering
        // Supports: Tenant user management, active user queries
        builder.HasIndex(u => new { u.TenantId, u.IsDeleted, u.CreatedAt })
            .HasDatabaseName("IX_Users_TenantId_IsDeleted_CreatedAt");

        // 4. TENANT EMAIL INDEX - Tenant-scoped email lookups
        // Supports: User search within tenant by email
        builder.HasIndex(u => new { u.TenantId, u.Email })
            .HasDatabaseName("IX_Users_TenantId_Email");

        // 5. TENANT USERNAME INDEX - Tenant-scoped username lookups
        // Supports: Username availability checks within tenant
        builder.HasIndex(u => new { u.TenantId, u.MeetlrUsername })
            .HasDatabaseName("IX_Users_TenantId_MeetlrUsername")
            .HasFilter("[MeetlrUsername] IS NOT NULL");

        // Relationships
        builder.HasMany(u => u.AvailabilitySchedules)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.MeetlrEvents)
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Bookings)
            .WithOne(b => b.HostUser)
            .HasForeignKey(b => b.HostUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // CalendarIntegrations relationship is now configured in the Calendar plugin

        // Note: User.Tenant relationship is configured in TenantConfiguration
        // to avoid duplicate FK (TenantId1) shadow property warning

        builder.HasMany(u => u.UserGroups)
            .WithOne(ug => ug.User)
            .HasForeignKey(ug => ug.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.ProvidedServices)
            .WithOne(s => s.Provider)
            .HasForeignKey(s => s.ProviderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
