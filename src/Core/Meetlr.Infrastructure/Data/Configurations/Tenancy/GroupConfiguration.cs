using  Meetlr.Domain.Entities.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Infrastructure.Data.Configurations.Tenancy;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("Groups");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(g => g.Description)
            .HasMaxLength(1000);

        // ============================================
        // OPTIMIZED INDEX STRATEGY - 3 Strategic Indexes
        // ============================================

        // 1. UNIQUE GROUP NAME INDEX - Tenant-scoped group name uniqueness
        // Supports: Group creation validation, name uniqueness checks
        builder.HasIndex(g => new { g.TenantId, g.Name })
            .IsUnique()
            .HasDatabaseName("IX_Groups_TenantId_Name");

        // 2. TENANT GROUPS INDEX - Tenant's groups with admin filtering and sorting
        // Supports: Group list, admin group identification, creation date sorting
        builder.HasIndex(g => new { g.TenantId, g.IsAdminGroup, g.CreatedAt })
            .HasDatabaseName("IX_Groups_TenantId_IsAdminGroup_CreatedAt");

        // 3. ADMIN GROUP LOOKUP INDEX - Fast admin group identification
        // Supports: Permission checks, admin group queries
        builder.HasIndex(g => new { g.TenantId, g.IsAdminGroup })
            .HasDatabaseName("IX_Groups_TenantId_IsAdminGroup")
            .HasFilter("[IsAdminGroup] = 1");

        // Relationships
        builder.HasOne(g => g.Tenant)
            .WithMany(t => t.Groups)
            .HasForeignKey(g => g.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(g => g.UserGroups)
            .WithOne(ug => ug.Group)
            .HasForeignKey(ug => ug.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
