using Meetlr.Application.Common.Specifications;
using Meetlr.Module.Calendar.Domain.Entities;
using Meetlr.Module.Calendar.Domain.Enums;

namespace Meetlr.Module.Calendar.Application.Specifications;

/// <summary>
/// Specifications for CalendarIntegration queries
/// Calendar integrations are now linked at the schedule level (AvailabilityScheduleId)
/// </summary>
public static class CalendarIntegrationSpecifications
{
    public class ByScheduleAndProvider : BaseSpecification<CalendarIntegration>
    {
        public ByScheduleAndProvider(Guid scheduleId, CalendarProvider provider)
            : base(c => c.AvailabilityScheduleId == scheduleId && c.Provider == provider)
        {
        }
    }

    public class ActiveBySchedule : BaseSpecification<CalendarIntegration>
    {
        public ActiveBySchedule(Guid scheduleId)
            : base(c => c.AvailabilityScheduleId == scheduleId && c.IsActive)
        {
        }
    }

    public class ActiveByScheduleWithRefreshToken : BaseSpecification<CalendarIntegration>
    {
        public ActiveByScheduleWithRefreshToken(Guid scheduleId)
            : base(c => c.AvailabilityScheduleId == scheduleId && c.IsActive && c.RefreshToken != null)
        {
        }
    }

    public class ByScheduleWithAutoSync : BaseSpecification<CalendarIntegration>
    {
        public ByScheduleWithAutoSync(Guid scheduleId)
            : base(c => c.AvailabilityScheduleId == scheduleId && c.IsActive && c.AutoSync)
        {
        }
    }

    public class ById : BaseSpecification<CalendarIntegration>
    {
        public ById(Guid id)
            : base(c => c.Id == id)
        {
        }
    }
}
