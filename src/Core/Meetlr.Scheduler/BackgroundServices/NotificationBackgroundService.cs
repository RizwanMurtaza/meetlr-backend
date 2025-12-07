using Meetlr.Scheduler.Services.Notifications;
using Meetlr.Scheduler.Services.ScheduledTasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Meetlr.Scheduler.BackgroundServices;

/// <summary>
/// Background service that continuously polls for pending notifications
/// and runs scheduled maintenance tasks.
/// </summary>
public class NotificationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationBackgroundService> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(10);

    public NotificationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<NotificationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification Processor Background Service started");

        // Create singleton instances for stateful services
        var scheduledTaskRunner = new ScheduledTaskRunner(
            _serviceProvider,
            _serviceProvider.GetRequiredService<ILogger<ScheduledTaskRunner>>());

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var notificationProcessor = scope.ServiceProvider.GetRequiredService<INotificationProcessingService>();

                await notificationProcessor.ProcessPendingNotificationsAsync(stoppingToken);
                await scheduledTaskRunner.RunScheduledTasksAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in background service execution cycle");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("Notification Processor Background Service stopped");
    }
}
