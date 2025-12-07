using Meetlr.Domain.Entities.Emails;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Module.Notifications.Infrastructure.Data.Configurations;

public class EmailProviderConfigurationConfiguration : IEntityTypeConfiguration<EmailProviderConfiguration>
{
    public void Configure(EntityTypeBuilder<EmailProviderConfiguration> builder)
    {
        builder.ToTable("EmailProviderConfigurations");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ProviderType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.ApiKey)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.DefaultFromEmail)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.DefaultFromName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.IsActive)
            .IsRequired();

        builder.Property(e => e.IsSystemDefault)
            .IsRequired();

        builder.Property(e => e.LastTestError)
            .HasMaxLength(2000);

        builder.Property(e => e.AdditionalSettings)
            .HasColumnType("text");

        // Indexes for performance
        builder.HasIndex(e => e.ProviderType);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => new { e.ProviderType, e.IsActive })
            .HasDatabaseName("IX_EmailProviderConfigurations_Provider_Active");
        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => new { e.TenantId, e.UserId, e.ProviderType })
            .HasDatabaseName("IX_EmailProviderConfigurations_Hierarchy");

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
