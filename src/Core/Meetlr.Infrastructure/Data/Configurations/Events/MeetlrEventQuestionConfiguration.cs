using  Meetlr.Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Infrastructure.Data.Configurations.Events;

public class MeetlrEventQuestionConfiguration : IEntityTypeConfiguration<MeetlrEventQuestion>
{
    public void Configure(EntityTypeBuilder<MeetlrEventQuestion> builder)
    {
        builder.ToTable("MeetlrEventQuestions");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.QuestionText)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(q => q.Type)
            .IsRequired();

        builder.Property(q => q.IsRequired)
            .IsRequired();

        builder.Property(q => q.DisplayOrder)
            .IsRequired();

        builder.Property(q => q.Options)
            .HasMaxLength(2000);

        // Indexes
        builder.HasIndex(q => q.MeetlrEventId)
            .HasDatabaseName("IX_MeetlrEventQuestions_MeetlrEventId");

        builder.HasIndex(q => new { q.MeetlrEventId, q.DisplayOrder })
            .HasDatabaseName("IX_MeetlrEventQuestions_MeetlrEventId_DisplayOrder");

        builder.HasIndex(q => new { q.MeetlrEventId, q.IsRequired })
            .HasDatabaseName("IX_MeetlrEventQuestions_MeetlrEventId_IsRequired");

        builder.HasIndex(q => new { q.MeetlrEventId, q.Type })
            .HasDatabaseName("IX_MeetlrEventQuestions_MeetlrEventId_Type");

        // Relationships
        builder.HasOne(q => q.MeetlrEvent)
            .WithMany(e => e.Questions)
            .HasForeignKey(q => q.MeetlrEventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(q => q.Answers)
            .WithOne(a => a.Question)
            .HasForeignKey(a => a.MeetlrEventQuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
