using Meetlr.Domain.Entities.Emails;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Module.Notifications.Infrastructure.Data.Configurations;

public class EmailConfigurationConfiguration : IEntityTypeConfiguration<EmailConfiguration>
{
    public void Configure(EntityTypeBuilder<EmailConfiguration> builder)
    {
        builder.ToTable("EmailConfigurations");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.SmtpHost)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.SmtpPort)
            .IsRequired();

        builder.Property(e => e.SmtpUsername)
            .IsRequired()
            .HasMaxLength(500);  // Oracle Cloud OCID usernames can be very long

        builder.Property(e => e.SmtpPassword)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.FromEmail)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.FromName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.EnableSsl)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.IsSystemDefault)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.LastTestedAt);

        builder.Property(e => e.LastTestSucceeded);

        builder.Property(e => e.LastTestError)
            .HasMaxLength(1000);

        // Indexes for efficient querying
        builder.HasIndex(e => new { e.TenantId, e.UserId, e.IsActive })
            .HasDatabaseName("IX_EmailConfigurations_Hierarchy_Active");

        builder.HasIndex(e => e.IsSystemDefault)
            .HasDatabaseName("IX_EmailConfigurations_IsSystemDefault");

        builder.HasIndex(e => e.FromEmail)
            .HasDatabaseName("IX_EmailConfigurations_FromEmail");

        // Relationships
        builder.HasOne(e => e.Tenant)
            .WithMany()
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
