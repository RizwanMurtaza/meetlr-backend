using System.Text.Json.Serialization;
using Meetlr.Domain.Common;

namespace Meetlr.Domain.Entities.Scheduling;

public class DateSpecificHours : BaseEntity
{
    public Guid AvailabilityScheduleId { get; set; }
    public DateTime Date { get; set; }
    public bool IsAvailable { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }

    // Navigation properties
    [JsonIgnore]
    public AvailabilitySchedule AvailabilitySchedule { get; set; } = null!;
}
