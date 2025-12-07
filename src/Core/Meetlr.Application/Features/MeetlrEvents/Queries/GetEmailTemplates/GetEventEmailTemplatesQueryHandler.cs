using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Emails;
using Meetlr.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.MeetlrEvents.Queries.GetEmailTemplates;

public class GetEventEmailTemplatesQueryHandler : IRequestHandler<GetEventEmailTemplatesQuery, List<GetEventEmailTemplatesResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetEventEmailTemplatesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<GetEventEmailTemplatesResponse>> Handle(GetEventEmailTemplatesQuery request, CancellationToken cancellationToken)
    {
        // Get custom templates for this event
        var customTemplates = await _unitOfWork.Repository<EventEmailTemplate>()
            .GetQueryable()
            .Where(t => t.MeetlrEventId == request.MeetlrEventId && !t.IsDeleted)
            .ToListAsync(cancellationToken);

        var customTemplateDict = customTemplates.ToDictionary(t => t.TemplateType);

        // Define the template types relevant for event customization
        var relevantTypes = new[]
        {
            EmailTemplateType.BookingConfirmationInvitee,
            EmailTemplateType.BookingReminder,
            EmailTemplateType.BookingCancellationInvitee,
            EmailTemplateType.BookingRescheduled,
            EmailTemplateType.GeneralEmail
        };

        var result = new List<GetEventEmailTemplatesResponse>();

        foreach (var templateType in relevantTypes)
        {
            if (customTemplateDict.TryGetValue(templateType, out var customTemplate))
            {
                // Return custom template
                result.Add(new GetEventEmailTemplatesResponse
                {
                    Id = customTemplate.Id,
                    MeetlrEventId = request.MeetlrEventId,
                    TemplateType = templateType,
                    Subject = customTemplate.Subject,
                    HtmlBody = customTemplate.HtmlBody,
                    IsCustom = true,
                    IsActive = customTemplate.IsActive
                });
            }
            else
            {
                // Return default template placeholder
                var defaultTemplate = GetDefaultTemplate(templateType);
                result.Add(new GetEventEmailTemplatesResponse
                {
                    Id = null,
                    MeetlrEventId = request.MeetlrEventId,
                    TemplateType = templateType,
                    Subject = defaultTemplate.Subject,
                    HtmlBody = defaultTemplate.HtmlBody,
                    IsCustom = false,
                    IsActive = true
                });
            }
        }

        return result;
    }

    private static (string Subject, string HtmlBody) GetDefaultTemplate(EmailTemplateType templateType)
    {
        return templateType switch
        {
            EmailTemplateType.BookingConfirmationInvitee => (
                "Booking Confirmed: {{eventName}}",
                "<p>Hi {{inviteeName}},</p><p>Your booking has been confirmed!</p><p><strong>Event:</strong> {{eventName}}</p><p><strong>Date:</strong> {{bookingDate}}</p><p><strong>Time:</strong> {{bookingTime}}</p><p><strong>Location:</strong> {{location}}</p><p>We look forward to seeing you!</p>"
            ),
            EmailTemplateType.BookingReminder => (
                "Reminder: {{eventName}} is coming up",
                "<p>Hi {{inviteeName}},</p><p>This is a friendly reminder about your upcoming booking.</p><p><strong>Event:</strong> {{eventName}}</p><p><strong>Date:</strong> {{bookingDate}}</p><p><strong>Time:</strong> {{bookingTime}}</p><p><strong>Location:</strong> {{location}}</p><p>See you soon!</p>"
            ),
            EmailTemplateType.BookingCancellationInvitee => (
                "Booking Cancelled: {{eventName}}",
                "<p>Hi {{inviteeName}},</p><p>Your booking for {{eventName}} on {{bookingDate}} has been cancelled.</p><p>If you have any questions, please contact us.</p>"
            ),
            EmailTemplateType.BookingRescheduled => (
                "Booking Rescheduled: {{eventName}}",
                "<p>Hi {{inviteeName}},</p><p>Your booking has been rescheduled.</p><p><strong>Event:</strong> {{eventName}}</p><p><strong>New Date:</strong> {{bookingDate}}</p><p><strong>New Time:</strong> {{bookingTime}}</p><p><strong>Location:</strong> {{location}}</p>"
            ),
            EmailTemplateType.GeneralEmail => (
                "Message from {{hostName}}",
                "<p>Hi {{inviteeName}},</p><p>You have a new message regarding {{eventName}}.</p>"
            ),
            _ => ("Email", "<p>Email content</p>")
        };
    }
}
