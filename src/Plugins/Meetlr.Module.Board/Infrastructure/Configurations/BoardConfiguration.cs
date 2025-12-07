using Meetlr.Module.Board.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Module.Board.Infrastructure.Configurations;

public class BoardConfiguration : IEntityTypeConfiguration<Domain.Entities.Board>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Board> builder)
    {
        builder.ToTable("Boards");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Description)
            .HasMaxLength(1000);

        builder.Property(b => b.Color)
            .HasMaxLength(20);

        builder.Property(b => b.Position)
            .IsRequired()
            .HasDefaultValue(0);

        // Indexes
        builder.HasIndex(b => new { b.TenantId, b.UserId, b.IsDeleted })
            .HasDatabaseName("IX_Boards_TenantId_UserId");

        builder.HasIndex(b => new { b.UserId, b.Position })
            .HasDatabaseName("IX_Boards_UserId_Position");

        // Relationships
        builder.HasOne(b => b.Owner)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Columns)
            .WithOne(c => c.Board)
            .HasForeignKey(c => c.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.Labels)
            .WithOne(l => l.Board)
            .HasForeignKey(l => l.BoardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
