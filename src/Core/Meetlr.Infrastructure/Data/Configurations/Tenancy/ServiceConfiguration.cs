using  Meetlr.Domain.Entities.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Infrastructure.Data.Configurations.Tenancy;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("Services");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .HasMaxLength(2000);

        builder.Property(s => s.DurationMinutes)
            .HasDefaultValue(30);

        builder.Property(s => s.Price)
            .HasPrecision(18, 2)
            .HasDefaultValue(0);

        builder.Property(s => s.Currency)
            .HasMaxLength(10)
            .HasDefaultValue("USD");

        builder.Property(s => s.ImageUrl)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(s => s.TenantId)
            .HasDatabaseName("IX_Services_TenantId");

        builder.HasIndex(s => s.ProviderId)
            .HasDatabaseName("IX_Services_ProviderId");

        builder.HasIndex(s => new { s.TenantId, s.Name })
            .HasDatabaseName("IX_Services_TenantId_Name");

        builder.HasIndex(s => s.IsActive)
            .HasDatabaseName("IX_Services_IsActive");

        builder.HasIndex(s => new { s.TenantId, s.IsActive })
            .HasDatabaseName("IX_Services_TenantId_IsActive");

        // Relationships
        builder.HasOne(s => s.Tenant)
            .WithMany(t => t.Services)
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Provider)
            .WithMany(u => u.ProvidedServices)
            .HasForeignKey(s => s.ProviderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
