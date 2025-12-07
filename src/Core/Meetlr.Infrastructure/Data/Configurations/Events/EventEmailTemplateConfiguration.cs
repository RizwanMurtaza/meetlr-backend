using Meetlr.Domain.Entities.Emails;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Infrastructure.Data.Configurations.Events;

public class EventEmailTemplateConfiguration : IEntityTypeConfiguration<EventEmailTemplate>
{
    public void Configure(EntityTypeBuilder<EventEmailTemplate> builder)
    {
        builder.ToTable("EventEmailTemplates");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.MeetlrEventId)
            .IsRequired();

        builder.Property(e => e.TemplateType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Subject)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.HtmlBody)
            .IsRequired();

        builder.Property(e => e.PlainTextBody);

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Indexes
        // Unique constraint: one template per event per type
        builder.HasIndex(e => new { e.MeetlrEventId, e.TemplateType })
            .IsUnique()
            .HasDatabaseName("IX_EventEmailTemplates_MeetlrEventId_TemplateType");

        // Soft delete filter
        builder.HasQueryFilter(e => !e.IsDeleted);

        // Relationships
        builder.HasOne(e => e.MeetlrEvent)
            .WithMany()
            .HasForeignKey(e => e.MeetlrEventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
