namespace Meetlr.Application.Plugins.Services;

/// <summary>
/// Service interface for seeding default email templates when an event is created.
/// Implemented in the Notifications module.
/// </summary>
public interface IEventEmailTemplateSeeder
{
    /// <summary>
    /// Seeds default email templates for a newly created event.
    /// Creates copies of the booking-related templates (confirmation, cancellation, reminder, rescheduled)
    /// so the event owner can customize them.
    /// </summary>
    /// <param name="eventId">The ID of the newly created event</param>
    /// <param name="tenantId">The tenant ID for the event</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SeedDefaultTemplatesForEventAsync(Guid eventId, Guid tenantId, CancellationToken cancellationToken = default);
}
