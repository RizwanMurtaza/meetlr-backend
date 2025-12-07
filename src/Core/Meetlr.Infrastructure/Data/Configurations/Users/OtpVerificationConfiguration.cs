using  Meetlr.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Infrastructure.Data.Configurations.Users;

public class OtpVerificationConfiguration : IEntityTypeConfiguration<OtpVerification>
{
    public void Configure(EntityTypeBuilder<OtpVerification> builder)
    {
        builder.ToTable("OtpVerifications");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(e => e.Purpose)
            .IsRequired();

        builder.Property(e => e.ExpiresAt)
            .IsRequired();

        builder.Property(e => e.IsUsed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.UsedAt);

        builder.Property(e => e.AttemptCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.LastAttemptAt);

        // Indexes for efficient querying
        builder.HasIndex(e => new { e.UserId, e.Code, e.Purpose, e.IsUsed })
            .HasDatabaseName("IX_OtpVerifications_Lookup");

        builder.HasIndex(e => e.ExpiresAt)
            .HasDatabaseName("IX_OtpVerifications_ExpiresAt");

        builder.HasIndex(e => new { e.UserId, e.Purpose, e.IsUsed })
            .HasDatabaseName("IX_OtpVerifications_User_Purpose");

        // Relationship
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
