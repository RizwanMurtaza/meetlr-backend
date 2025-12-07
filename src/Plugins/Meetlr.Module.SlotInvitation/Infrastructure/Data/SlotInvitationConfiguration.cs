using Meetlr.Module.SlotInvitation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Module.SlotInvitation.Infrastructure.Data;

public class SlotInvitationConfiguration : IEntityTypeConfiguration<Domain.Entities.SlotInvitation>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.SlotInvitation> builder)
    {
        builder.ToTable("SlotInvitations");

        builder.HasKey(s => s.Id);

        // Slot Details
        builder.Property(s => s.SlotStartTime)
            .IsRequired();

        builder.Property(s => s.SlotEndTime)
            .IsRequired();

        builder.Property(s => s.SpotsReserved)
            .IsRequired()
            .HasDefaultValue(1);

        // Invitation Details
        builder.Property(s => s.Token)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(s => s.InviteeEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(s => s.InviteeName)
            .HasMaxLength(256);

        builder.Property(s => s.ExpiresAt)
            .IsRequired();

        builder.Property(s => s.ExpirationHours)
            .IsRequired();

        // Status Tracking
        builder.Property(s => s.Status)
            .IsRequired();

        // Email Tracking
        builder.Property(s => s.EmailStatus)
            .IsRequired();

        builder.Property(s => s.EmailAttempts)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.EmailError)
            .HasMaxLength(1000);

        // INDEXES

        // 1. UNIQUE TOKEN INDEX - For public URL lookup
        builder.HasIndex(s => s.Token)
            .IsUnique()
            .HasDatabaseName("IX_SlotInvitations_Token");

        // 2. EXPIRATION INDEX - For background job to expire invitations
        builder.HasIndex(s => new { s.Status, s.ExpiresAt })
            .HasDatabaseName("IX_SlotInvitations_Status_ExpiresAt");

        // 3. EVENT + STATUS INDEX - For listing invitations by event
        builder.HasIndex(s => new { s.MeetlrEventId, s.Status })
            .HasDatabaseName("IX_SlotInvitations_MeetlrEventId_Status");

        // 4. USER INDEX - For user's invitations across all events
        builder.HasIndex(s => s.UserId)
            .HasDatabaseName("IX_SlotInvitations_UserId");

        // 5. AVAILABILITY CHECK INDEX - For checking reserved slots
        builder.HasIndex(s => new { s.MeetlrEventId, s.Status, s.SlotStartTime, s.SlotEndTime })
            .HasDatabaseName("IX_SlotInvitations_Availability");
    }
}
