using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Plugins.Services;
using Meetlr.Domain.Entities.Emails;
using Meetlr.Domain.Enums;
using Meetlr.Module.Notifications.Infrastructure.Data.Seeding;
using Microsoft.Extensions.Logging;

namespace Meetlr.Module.Notifications.Infrastructure.Services;

/// <summary>
/// Seeds default email templates when a new event is created.
/// Creates copies of booking-related templates for event customization.
/// </summary>
public class EventEmailTemplateSeeder : IEventEmailTemplateSeeder
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EventEmailTemplateSeeder> _logger;

    // Template types that are relevant to events (booking-related)
    private static readonly EmailTemplateType[] EventTemplateTypes =
    [
        EmailTemplateType.BookingConfirmationHost,
        EmailTemplateType.BookingConfirmationInvitee,
        EmailTemplateType.BookingCancellationHost,
        EmailTemplateType.BookingCancellationInvitee,
        EmailTemplateType.BookingReminder,
        EmailTemplateType.BookingRescheduled
    ];

    public EventEmailTemplateSeeder(
        IUnitOfWork unitOfWork,
        ILogger<EventEmailTemplateSeeder> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SeedDefaultTemplatesForEventAsync(
        Guid eventId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Seeding default email templates for event {EventId}", eventId);

        try
        {
            var defaultTemplates = DefaultEmailTemplates.GetDefaults();

            foreach (var templateType in EventTemplateTypes)
            {
                if (!defaultTemplates.TryGetValue(templateType, out var templateData))
                {
                    _logger.LogWarning("No default template found for type {TemplateType}", templateType);
                    continue;
                }

                var (subject, htmlBody, plainText, _) = templateData;

                var eventTemplate = new EventEmailTemplate
                {
                    Id = Guid.NewGuid(),
                    MeetlrEventId = eventId,
                    TenantId = tenantId,
                    TemplateType = templateType,
                    Subject = subject,
                    HtmlBody = htmlBody,
                    PlainTextBody = plainText,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };

                _unitOfWork.Repository<EventEmailTemplate>().Add(eventTemplate);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully seeded {Count} email templates for event {EventId}",
                EventTemplateTypes.Length,
                eventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed email templates for event {EventId}", eventId);
            // Don't throw - email templates are not critical for event creation
        }
    }
}
