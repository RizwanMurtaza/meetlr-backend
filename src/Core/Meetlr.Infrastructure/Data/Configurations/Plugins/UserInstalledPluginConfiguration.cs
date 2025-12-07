using  Meetlr.Domain.Entities.Plugins;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Infrastructure.Data.Configurations.Plugins;

public class UserInstalledPluginConfiguration : IEntityTypeConfiguration<UserInstalledPlugin>
{
    public void Configure(EntityTypeBuilder<UserInstalledPlugin> builder)
    {
        builder.ToTable("UserInstalledPlugins");

        builder.HasKey(p => p.Id);

        // Required fields
        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.PluginCategory)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.PluginId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.PluginName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.PluginVersion)
            .HasMaxLength(20);

        builder.Property(p => p.ConnectionStatus)
            .HasMaxLength(50);

        builder.Property(p => p.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.IsConnected)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.InstalledAt)
            .IsRequired();

        builder.Property(p => p.UsageCount)
            .IsRequired()
            .HasDefaultValue(0);

        // JSON columns for flexible data storage
        builder.Property(p => p.Settings)
            .HasColumnType("longtext");

        builder.Property(p => p.Metadata)
            .HasColumnType("longtext");

        builder.Property(p => p.ErrorMessage)
            .HasColumnType("text");

        // Indexes for common queries
        builder.HasIndex(p => p.UserId)
            .HasDatabaseName("IX_UserInstalledPlugins_UserId");

        builder.HasIndex(p => new { p.PluginCategory, p.PluginId })
            .HasDatabaseName("IX_UserInstalledPlugins_Category_PluginId");

        // Unique constraint: One user can install each plugin only once (considering soft deletes)
        builder.HasIndex(p => new { p.UserId, p.PluginCategory, p.PluginId, p.IsDeleted })
            .IsUnique()
            .HasDatabaseName("IX_UserInstalledPlugins_User_Plugin");

        builder.HasIndex(p => new { p.UserId, p.PluginCategory, p.IsEnabled, p.IsConnected })
            .HasDatabaseName("IX_UserInstalledPlugins_UserId_Category_Enabled_Connected");

        builder.HasIndex(p => new { p.UserId, p.IsEnabled })
            .HasDatabaseName("IX_UserInstalledPlugins_UserId_IsEnabled");

        builder.HasIndex(p => new { p.UserId, p.IsConnected })
            .HasDatabaseName("IX_UserInstalledPlugins_UserId_IsConnected");

        builder.HasIndex(p => p.InstalledAt)
            .HasDatabaseName("IX_UserInstalledPlugins_InstalledAt");

        builder.HasIndex(p => p.LastUsedAt)
            .HasDatabaseName("IX_UserInstalledPlugins_LastUsedAt");

        // Foreign key to User
        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
