using Meetlr.Domain.Enums;
using Meetlr.Module.Board.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Module.Board.Infrastructure.Configurations;

public class BoardTaskConfiguration : IEntityTypeConfiguration<BoardTask>
{
    public void Configure(EntityTypeBuilder<BoardTask> builder)
    {
        builder.ToTable("BoardTasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(t => t.Description)
            .HasColumnType("text");

        builder.Property(t => t.Position)
            .IsRequired()
            .HasDefaultValue(0);

        // Entity already defaults to Medium in the class definition.
        // No DB default needed - this avoids sentinel value warnings and allows Low priority to be set.
        builder.Property(t => t.Priority)
            .IsRequired()
            .HasConversion<int>();

        // Indexes
        builder.HasIndex(t => new { t.ColumnId, t.Position })
            .HasDatabaseName("IX_BoardTasks_ColumnId_Position");

        builder.HasIndex(t => new { t.ColumnId, t.IsDeleted })
            .HasDatabaseName("IX_BoardTasks_ColumnId_IsDeleted");

        builder.HasIndex(t => t.DueDate)
            .HasDatabaseName("IX_BoardTasks_DueDate")
            .HasFilter("[DueDate] IS NOT NULL");

        // Many-to-many with labels configured via join table
        builder.HasMany(t => t.Labels)
            .WithMany(l => l.Tasks)
            .UsingEntity<BoardTaskLabel>(
                j => j.HasOne(tl => tl.Label)
                    .WithMany()
                    .HasForeignKey(tl => tl.LabelId)
                    .OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne(tl => tl.Task)
                    .WithMany()
                    .HasForeignKey(tl => tl.TaskId)
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.ToTable("BoardTaskLabels");
                    j.HasKey(tl => new { tl.TaskId, tl.LabelId });
                }
            );
    }
}
