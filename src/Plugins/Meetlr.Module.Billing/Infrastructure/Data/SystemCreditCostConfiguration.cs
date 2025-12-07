using Meetlr.Module.Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Module.Billing.Infrastructure.Data;

public class SystemCreditCostConfiguration : IEntityTypeConfiguration<SystemCreditCost>
{
    public void Configure(EntityTypeBuilder<SystemCreditCost> builder)
    {
        builder.ToTable("SystemCreditCosts");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.ServiceType)
            .IsRequired();

        builder.Property(s => s.CreditCost)
            .IsRequired();

        builder.Property(s => s.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // INDEXES

        // Unique index on ServiceType - only one cost per service type
        builder.HasIndex(s => s.ServiceType)
            .IsUnique()
            .HasDatabaseName("IX_SystemCreditCosts_ServiceType");
    }
}
