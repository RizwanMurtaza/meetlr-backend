using  Meetlr.Domain.Entities.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Infrastructure.Data.Configurations.Tenancy;

public class UserGroupConfiguration : IEntityTypeConfiguration<UserGroup>
{
    public void Configure(EntityTypeBuilder<UserGroup> builder)
    {
        builder.ToTable("UserGroups");

        builder.HasKey(ug => ug.Id);

        // Indexes
        builder.HasIndex(ug => ug.UserId)
            .HasDatabaseName("IX_UserGroups_UserId");

        builder.HasIndex(ug => ug.GroupId)
            .HasDatabaseName("IX_UserGroups_GroupId");

        builder.HasIndex(ug => new { ug.UserId, ug.GroupId })
            .IsUnique()
            .HasDatabaseName("IX_UserGroups_UserId_GroupId");

        builder.HasIndex(ug => ug.CreatedAt)
            .HasDatabaseName("IX_UserGroups_CreatedAt");

        builder.HasIndex(ug => new { ug.UserId, ug.CreatedAt })
            .HasDatabaseName("IX_UserGroups_UserId_CreatedAt");

        builder.HasIndex(ug => new { ug.GroupId, ug.CreatedAt })
            .HasDatabaseName("IX_UserGroups_GroupId_CreatedAt");

        // Relationships
        builder.HasOne(ug => ug.User)
            .WithMany(u => u.UserGroups)
            .HasForeignKey(ug => ug.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ug => ug.Group)
            .WithMany(g => g.UserGroups)
            .HasForeignKey(ug => ug.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
