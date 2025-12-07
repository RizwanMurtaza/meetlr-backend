using Meetlr.Module.Analytics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Module.Analytics.Infrastructure.Configurations;

/// <summary>
/// Entity Framework configuration for the PageView entity
/// </summary>
public class PageViewConfiguration : IEntityTypeConfiguration<PageView>
{
    public void Configure(EntityTypeBuilder<PageView> builder)
    {
        builder.ToTable("PageViews");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PagePath)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(p => p.Username)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.EventSlug)
            .HasMaxLength(200);

        builder.Property(p => p.SessionId)
            .HasMaxLength(100);

        builder.Property(p => p.UserAgent)
            .HasMaxLength(500);

        builder.Property(p => p.ReferrerUrl)
            .HasMaxLength(1000);

        builder.Property(p => p.DeviceType)
            .HasMaxLength(50);

        builder.Property(p => p.IpAddressHash)
            .HasMaxLength(64);

        builder.Property(p => p.CountryCode)
            .HasMaxLength(10);

        // Indexes for query performance
        builder.HasIndex(p => p.UserId)
            .HasDatabaseName("IX_PageViews_UserId");

        builder.HasIndex(p => p.MeetlrEventId)
            .HasDatabaseName("IX_PageViews_MeetlrEventId");

        builder.HasIndex(p => p.ViewedAt)
            .HasDatabaseName("IX_PageViews_ViewedAt");

        builder.HasIndex(p => p.TenantId)
            .HasDatabaseName("IX_PageViews_TenantId");

        // Composite index for user analytics queries
        builder.HasIndex(p => new { p.UserId, p.ViewedAt })
            .HasDatabaseName("IX_PageViews_UserId_ViewedAt");

        // Composite index for event analytics queries
        builder.HasIndex(p => new { p.MeetlrEventId, p.ViewedAt })
            .HasDatabaseName("IX_PageViews_MeetlrEventId_ViewedAt");

        // Index for unique visitor tracking
        builder.HasIndex(p => new { p.UserId, p.SessionId, p.ViewedAt })
            .HasDatabaseName("IX_PageViews_UserId_SessionId_ViewedAt");
    }
}
