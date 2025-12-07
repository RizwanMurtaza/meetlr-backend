using Meetlr.Module.Board.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Module.Board.Infrastructure.Configurations;

public class BoardColumnConfiguration : IEntityTypeConfiguration<BoardColumn>
{
    public void Configure(EntityTypeBuilder<BoardColumn> builder)
    {
        builder.ToTable("BoardColumns");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Position)
            .IsRequired()
            .HasDefaultValue(0);

        // Indexes
        builder.HasIndex(c => new { c.BoardId, c.Position })
            .HasDatabaseName("IX_BoardColumns_BoardId_Position");

        builder.HasIndex(c => new { c.BoardId, c.IsDeleted })
            .HasDatabaseName("IX_BoardColumns_BoardId_IsDeleted");

        // Relationships
        builder.HasMany(c => c.Tasks)
            .WithOne(t => t.Column)
            .HasForeignKey(t => t.ColumnId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
