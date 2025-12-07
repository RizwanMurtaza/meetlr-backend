using  Meetlr.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Infrastructure.Data.Configurations.Payments;

public class StripeAccountConfiguration : IEntityTypeConfiguration<StripeAccount>
{
    public void Configure(EntityTypeBuilder<StripeAccount> builder)
    {
        builder.ToTable("StripeAccounts");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.StripeAccountId)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(s => s.StripeAccountId)
            .IsUnique()
            .HasDatabaseName("IX_StripeAccounts_StripeAccountId");

        builder.HasIndex(s => s.UserId)
            .IsUnique()
            .HasDatabaseName("IX_StripeAccounts_UserId");

        builder.Property(s => s.Country)
            .HasMaxLength(2); // ISO country code

        builder.Property(s => s.Currency)
            .HasMaxLength(3); // ISO currency code

        builder.Property(s => s.Email)
            .HasMaxLength(256);

        builder.Property(s => s.BusinessType)
            .HasMaxLength(50);

        builder.Property(s => s.VerificationStatus)
            .HasMaxLength(50);

        builder.Property(s => s.DisabledReason)
            .HasMaxLength(500);

        builder.Property(s => s.Scope)
            .HasMaxLength(500);

        // Sensitive data - should be encrypted in production
        builder.Property(s => s.AccessToken)
            .HasMaxLength(1000);

        builder.Property(s => s.RefreshToken)
            .HasMaxLength(1000);

        // Indexes for query performance
        builder.HasIndex(s => s.ChargesEnabled)
            .HasDatabaseName("IX_StripeAccounts_ChargesEnabled");

        builder.HasIndex(s => s.VerificationStatus)
            .HasDatabaseName("IX_StripeAccounts_VerificationStatus");

        builder.HasIndex(s => new { s.UserId, s.ChargesEnabled })
            .HasDatabaseName("IX_StripeAccounts_UserId_ChargesEnabled");

        builder.HasIndex(s => s.IsConnected)
            .HasDatabaseName("IX_StripeAccounts_IsConnected");

        builder.HasIndex(s => s.CreatedAt)
            .HasDatabaseName("IX_StripeAccounts_CreatedAt");

        builder.HasIndex(s => new { s.UserId, s.IsConnected, s.ChargesEnabled })
            .HasDatabaseName("IX_StripeAccounts_UserId_IsConnected_ChargesEnabled");

        // Relationship with User
        builder.HasOne(s => s.User)
            .WithOne(u => u.StripeAccount)
            .HasForeignKey<StripeAccount>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
