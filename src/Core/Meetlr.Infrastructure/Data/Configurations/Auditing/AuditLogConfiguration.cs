using  Meetlr.Domain.Entities.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Infrastructure.Data.Configurations.Auditing;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.UserId)
            .HasMaxLength(450);

        builder.Property(a => a.UserEmail)
            .HasMaxLength(255);

        builder.Property(a => a.EntityName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.EntityId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.OldValues)
            .HasColumnType("text");

        builder.Property(a => a.NewValues)
            .HasColumnType("text");

        builder.Property(a => a.Changes)
            .HasColumnType("text");

        builder.Property(a => a.IpAddress)
            .HasMaxLength(45); // IPv6 max length

        builder.Property(a => a.UserAgent)
            .HasMaxLength(500);

        builder.Property(a => a.Timestamp)
            .IsRequired();

        // ============================================
        // OPTIMIZED INDEX STRATEGY - 4 Strategic Indexes
        // ============================================

        // 1. ENTITY AUDIT TRAIL INDEX - Complete history for a specific entity
        // Supports: Entity audit history, change tracking, compliance reports
        builder.HasIndex(a => new { a.EntityName, a.EntityId, a.Timestamp })
            .HasDatabaseName("IX_AuditLogs_EntityName_EntityId_Timestamp");

        // 2. USER ACTIVITY INDEX - User's audit trail with action filtering
        // Supports: User activity reports, security audits, action filtering
        builder.HasIndex(a => new { a.UserId, a.Action, a.Timestamp })
            .HasDatabaseName("IX_AuditLogs_UserId_Action_Timestamp");

        // 3. ENTITY TYPE INDEX - All audits for specific entity type with action filtering
        // Supports: Entity-level analytics (all Booking changes, all User changes, etc.)
        builder.HasIndex(a => new { a.EntityName, a.Action, a.Timestamp })
            .HasDatabaseName("IX_AuditLogs_EntityName_Action_Timestamp");

        // 4. TIMESTAMP INDEX - Time-based audit queries
        // Supports: Recent activity dashboard, date range queries, audit cleanup
        builder.HasIndex(a => a.Timestamp)
            .HasDatabaseName("IX_AuditLogs_Timestamp");
    }
}
