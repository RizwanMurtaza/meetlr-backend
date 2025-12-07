using  Meetlr.Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Infrastructure.Data.Configurations.Events;

public class BookingAnswerConfiguration : IEntityTypeConfiguration<BookingAnswer>
{
    public void Configure(EntityTypeBuilder<BookingAnswer> builder)
    {
        builder.ToTable("BookingAnswers");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Answer)
            .IsRequired()
            .HasMaxLength(2000);

        // ============================================
        // OPTIMIZED INDEX STRATEGY - 1 Strategic Index
        // ============================================

        // 1. UNIQUE BOOKING-QUESTION INDEX - One answer per question per booking
        // Supports: Answer creation validation, booking answers lookup, question statistics
        builder.HasIndex(a => new { a.BookingId, a.MeetlrEventQuestionId })
            .IsUnique()
            .HasDatabaseName("IX_BookingAnswers_BookingId_MeetlrEventQuestionId");

        // Relationships
        builder.HasOne(a => a.Booking)
            .WithMany(b => b.Answers)
            .HasForeignKey(a => a.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Question)
            .WithMany(q => q.Answers)
            .HasForeignKey(a => a.MeetlrEventQuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
