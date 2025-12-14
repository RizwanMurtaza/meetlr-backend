using System.Text.Json;
using Meetlr.Domain.Entities.Emails;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Plugins.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Module.Notifications.Infrastructure.Data.Seeding;

/// <summary>
/// Seeds system-level email templates
/// </summary>
public class EmailTemplateSeeder
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventEmailTemplateSeeder _eventEmailTemplateSeeder;
    private readonly ILogger<EmailTemplateSeeder> _logger;

    public EmailTemplateSeeder(
        IUnitOfWork unitOfWork,
        IEventEmailTemplateSeeder eventEmailTemplateSeeder,
        ILogger<EmailTemplateSeeder> logger)
    {
        _unitOfWork = unitOfWork;
        _eventEmailTemplateSeeder = eventEmailTemplateSeeder;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting email template seeding...");

        try
        {
            // Get all default templates
            var defaultTemplates = DefaultEmailTemplates.GetDefaults();

            // Get existing system templates
            var existingTemplates = await _unitOfWork.Repository<EmailTemplate>()
                .GetQueryable()
                .Where(t => t.IsSystemDefault)
                .ToListAsync(cancellationToken);

            var existingTemplatesByType = existingTemplates.ToDictionary(t => t.TemplateType);

            foreach (var (templateType, (subject, htmlBody, plainText, variables)) in defaultTemplates)
            {
                var variablesJson = JsonSerializer.Serialize(variables);

                if (existingTemplatesByType.TryGetValue(templateType, out var existingTemplate))
                {
                    // Update existing template if content has changed
                    if (existingTemplate.HtmlBody != htmlBody ||
                        existingTemplate.Subject != subject ||
                        existingTemplate.PlainTextBody != plainText ||
                        existingTemplate.AvailableVariablesJson != variablesJson)
                    {
                        existingTemplate.Subject = subject;
                        existingTemplate.HtmlBody = htmlBody;
                        existingTemplate.PlainTextBody = plainText;
                        existingTemplate.AvailableVariablesJson = variablesJson;

                        _unitOfWork.Repository<EmailTemplate>().Update(existingTemplate);
                        _logger.LogDebug("Updated system template for {TemplateType}", templateType);
                    }
                }
                else
                {
                    // Create new template
                    var template = new EmailTemplate
                    {
                        Id = Guid.NewGuid(),
                        TenantId = null,  // System level
                        UserId = null,    // System level
                        TemplateType = templateType,
                        Subject = subject,
                        HtmlBody = htmlBody,
                        PlainTextBody = plainText,
                        AvailableVariablesJson = variablesJson,
                        IsActive = true,
                        IsSystemDefault = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "System"
                    };

                    _unitOfWork.Repository<EmailTemplate>().Add(template);
                    _logger.LogDebug("Created system template for {TemplateType}", templateType);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("System email templates seeded/updated successfully ({Count} templates)", defaultTemplates.Count);

            // Also update event-level templates to ensure existing events get the latest defaults
            await _eventEmailTemplateSeeder.UpdateAllEventTemplatesToLatestAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding email templates");
            throw;
        }
    }
}
