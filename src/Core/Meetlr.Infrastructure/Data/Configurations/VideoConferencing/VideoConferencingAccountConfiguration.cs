using Meetlr.Domain.Entities.VideoConferencing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Infrastructure.Data.Configurations.VideoConferencing;

public class VideoConferencingAccountConfiguration : IEntityTypeConfiguration<VideoConferencingAccount>
{
    public void Configure(EntityTypeBuilder<VideoConferencingAccount> builder)
    {
        builder.ToTable("VideoConferencingAccounts");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Provider)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.ProviderAccountId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(v => v.Email)
            .HasMaxLength(256);

        // Sensitive data - should be encrypted in production
        builder.Property(v => v.AccessToken)
            .HasMaxLength(2000);

        builder.Property(v => v.RefreshToken)
            .HasMaxLength(2000);

        // Unique constraint: one account per user per provider
        builder.HasIndex(v => new { v.UserId, v.Provider })
            .IsUnique()
            .HasDatabaseName("IX_VideoConferencingAccounts_UserId_Provider");

        builder.HasIndex(v => v.ProviderAccountId)
            .HasDatabaseName("IX_VideoConferencingAccounts_ProviderAccountId");

        // Relationship with User
        builder.HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
