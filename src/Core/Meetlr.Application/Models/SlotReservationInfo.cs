namespace Meetlr.Application.Models;

/// <summary>
/// Lightweight DTO for availability queries to check reserved slots from SlotInvitation module.
/// This allows the core Availability handlers to check for reserved slots without
/// depending on the SlotInvitation module directly.
/// </summary>
public record SlotReservationInfo(
    DateTime StartTime,
    DateTime EndTime,
    int SpotsReserved
);
