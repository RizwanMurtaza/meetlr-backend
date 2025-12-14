using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Plugins.Services;
using Meetlr.Domain.Entities.Emails;
using Meetlr.Domain.Enums;
using Meetlr.Module.Notifications.Infrastructure.Data.Seeding;
using Microsoft.EntityFrameworkCore;
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

    public async Task UpdateAllEventTemplatesToLatestAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating all event email templates to latest defaults...");

        try
        {
            var defaultTemplates = DefaultEmailTemplates.GetDefaults();
            var updatedCount = 0;

            foreach (var templateType in EventTemplateTypes)
            {
                if (!defaultTemplates.TryGetValue(templateType, out var templateData))
                    continue;

                var (subject, htmlBody, plainText, _) = templateData;

                // Get all event templates of this type
                var eventTemplates = await _unitOfWork.Repository<EventEmailTemplate>()
                    .GetQueryable()
                    .Where(t => t.TemplateType == templateType)
                    .ToListAsync(cancellationToken);

                foreach (var template in eventTemplates)
                {
                    // Update to latest default
                    if (template.Subject != subject ||
                        template.HtmlBody != htmlBody ||
                        template.PlainTextBody != plainText)
                    {
                        template.Subject = subject;
                        template.HtmlBody = htmlBody;
                        template.PlainTextBody = plainText;
                        template.UpdatedAt = DateTime.UtcNow;
                        template.UpdatedBy = "System";

                        _unitOfWork.Repository<EventEmailTemplate>().Update(template);
                        updatedCount++;
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully updated {UpdatedCount} event email templates to latest defaults",
                updatedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update event email templates to latest defaults");
            // Don't throw - this is a maintenance operation
        }
    }
}
