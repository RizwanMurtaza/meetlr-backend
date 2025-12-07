namespace Meetlr.Domain.Enums;

/// <summary>
/// Defines the type of availability schedule
/// </summary>
public enum ScheduleType
{
    /// <summary>
    /// Personal schedule - one-on-one appointments (capacity = 1)
    /// </summary>
    Personal = 0,

    /// <summary>
    /// Business schedule - group appointments (capacity can be 1-10)
    /// </summary>
    Business = 1
}
