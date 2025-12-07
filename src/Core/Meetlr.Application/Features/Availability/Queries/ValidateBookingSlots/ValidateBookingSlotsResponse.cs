namespace Meetlr.Application.Features.Availability.Queries.ValidateBookingSlots;

public class ValidateBookingSlotsResponse
{
    public bool HasConflicts { get; set; }
    public List<SlotConflict> Conflicts { get; set; } = new();
    public string? Message { get; set; }
}

public class SlotConflict
{
    public int SlotIndex { get; set; } // 0-based index in the requested slots list
    public DateTime RequestedTime { get; set; }
    public string ConflictReason { get; set; } = string.Empty;
    public List<AlternativeSlotDto> SuggestedAlternatives { get; set; } = new();
}

public class AlternativeSlotDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string DisplayTime { get; set; } = string.Empty;
}
