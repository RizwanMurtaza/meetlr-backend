using Meetlr.Domain.Entities.Emails;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Module.Notifications.Infrastructure.Data.Configurations;

public class EmailTemplateConfiguration : IEntityTypeConfiguration<EmailTemplate>
{
    public void Configure(EntityTypeBuilder<EmailTemplate> builder)
    {
        builder.ToTable("EmailTemplates");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Subject)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.HtmlBody)
            .IsRequired();

        builder.Property(e => e.PlainTextBody);

        builder.Property(e => e.AvailableVariablesJson)
            .IsRequired()
            .HasDefaultValue("[]");

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.IsSystemDefault)
            .IsRequired()
            .HasDefaultValue(false);

        // Indexes for efficient querying
        builder.HasIndex(e => new { e.TenantId, e.UserId, e.TemplateType, e.IsActive })
            .HasDatabaseName("IX_EmailTemplates_Hierarchy_Lookup");

        builder.HasIndex(e => e.TemplateType)
            .HasDatabaseName("IX_EmailTemplates_TemplateType");

        builder.HasIndex(e => e.IsSystemDefault)
            .HasDatabaseName("IX_EmailTemplates_IsSystemDefault");

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
