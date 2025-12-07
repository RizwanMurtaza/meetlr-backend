using Meetlr.Module.Board.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Module.Board.Infrastructure.Configurations;

public class BoardLabelConfiguration : IEntityTypeConfiguration<BoardLabel>
{
    public void Configure(EntityTypeBuilder<BoardLabel> builder)
    {
        builder.ToTable("BoardLabels");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(l => l.Color)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("#6366f1");

        // Indexes
        builder.HasIndex(l => new { l.BoardId, l.IsDeleted })
            .HasDatabaseName("IX_BoardLabels_BoardId_IsDeleted");

        builder.HasIndex(l => new { l.BoardId, l.Name })
            .HasDatabaseName("IX_BoardLabels_BoardId_Name");
    }
}
