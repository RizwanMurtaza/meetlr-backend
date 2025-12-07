namespace Meetlr.Scheduler.Services.ScheduledTasks;

/// <summary>
/// Service responsible for running scheduled background tasks
/// like cleanup, verification, and expiration checks.
/// </summary>
public interface IScheduledTaskRunner
{
    /// <summary>
    /// Run all scheduled tasks that are due based on their intervals.
    /// </summary>
    Task RunScheduledTasksAsync(CancellationToken cancellationToken);
}
