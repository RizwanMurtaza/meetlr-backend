using Meetlr.Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Infrastructure.Data.Configurations.Events;

public class SingleUseBookingLinkConfiguration : IEntityTypeConfiguration<SingleUseBookingLink>
{
    public void Configure(EntityTypeBuilder<SingleUseBookingLink> builder)
    {
        builder.ToTable("SingleUseBookingLinks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Token)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.Name)
            .HasMaxLength(200);

        builder.HasIndex(x => x.Token)
            .IsUnique();

        builder.HasIndex(x => new { x.MeetlrEventId, x.IsActive });

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.MeetlrEvent)
            .WithMany()
            .HasForeignKey(x => x.MeetlrEventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Booking)
            .WithMany()
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
