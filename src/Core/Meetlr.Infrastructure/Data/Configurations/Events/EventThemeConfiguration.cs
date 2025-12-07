using Meetlr.Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Infrastructure.Data.Configurations.Events;

public class EventThemeConfiguration : IEntityTypeConfiguration<EventTheme>
{
    public void Configure(EntityTypeBuilder<EventTheme> builder)
    {
        builder.ToTable("EventThemes");

        builder.HasKey(x => x.Id);

        // Colors
        builder.Property(x => x.PrimaryColor)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.SecondaryColor)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.CalendarBackgroundColor)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.TextColor)
            .IsRequired()
            .HasMaxLength(20);

        // Typography
        builder.Property(x => x.FontFamily)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ButtonStyle)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.BorderRadius)
            .IsRequired();

        // Banner/Content
        builder.Property(x => x.BannerImageUrl)
            .HasMaxLength(500);

        builder.Property(x => x.BannerText)
            .HasMaxLength(500);

        builder.Property(x => x.CustomWelcomeMessage)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(x => x.MeetlrEventId)
            .IsUnique()
            .HasDatabaseName("IX_EventThemes_MeetlrEventId");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("IX_EventThemes_TenantId");

        // Relationships - One-to-One with MeetlrEvent
        builder.HasOne(x => x.MeetlrEvent)
            .WithOne(e => e.Theme)
            .HasForeignKey<EventTheme>(x => x.MeetlrEventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
