namespace Meetlr.Application.Features.Availability.Queries.GetAvailableSlots;

/// <summary>
/// Simple DTO for time slot conflicts
/// </summary>
internal record TimeSlotDto(DateTime StartTime, DateTime EndTime);