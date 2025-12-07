using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Common.Settings;
using Meetlr.Application.Interfaces;
using Meetlr.Application.Plugins.Services;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Entities.Plugins;
using Meetlr.Domain.Entities.Users;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Meetlr.Application.Features.MeetlrEvents.Commands.Create;

/// <summary>
/// Handler for creating event types
/// </summary>
public class CreateMeetlrEventCommandHandler : IRequestHandler<CreateMeetlrEventCommand, CreateMeetlrEventCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly ApplicationUrlsSettings _urlsSettings;
    private readonly IEventEmailTemplateSeeder? _emailTemplateSeeder;

    public CreateMeetlrEventCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IOptions<ApplicationUrlsSettings> urlsSettings,
        IEventEmailTemplateSeeder? emailTemplateSeeder = null)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _auditService = auditService;
        _urlsSettings = urlsSettings.Value;
        _emailTemplateSeeder = emailTemplateSeeder;
    }

    public async Task<CreateMeetlrEventCommandResponse> Handle(CreateMeetlrEventCommand request, CancellationToken cancellationToken)
    {
        // Get current user ID (guaranteed to be non-null by [Authorize] attribute on endpoint)
        var userId = _currentUserService.UserId!.Value;

        var user = await _unitOfWork.Repository<User>()
            .GetQueryable().Include(x=>x.UserSettings)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        //TODO right now user is restricted to 1 and only 
        
        if (user == null)
        {
            throw UserErrors.UserNotFound(userId);
        }

        // Validate payment provider if payment is required
        if (request.RequiresPayment)
        {
            if (request.PaymentProviderType == null)
            {
                throw PaymentErrors.PaymentProviderNotConfigured();
            }

            // Check if user has this provider installed and connected
            var providerPluginId = request.PaymentProviderType.ToString().ToLowerInvariant();
            var userPlugin = await _unitOfWork.Repository<UserInstalledPlugin>()
                .GetQueryable()
                .FirstOrDefaultAsync(p =>
                    p.UserId == userId &&
                    p.PluginId == providerPluginId &&
                    p.PluginCategory == PluginCategory.Payment &&
                    p.IsEnabled &&
                    p.IsConnected &&
                    !p.IsDeleted,
                    cancellationToken);

            if (userPlugin == null)
            {
                throw PluginErrors.PluginNotConnectedOrNotInstalled(providerPluginId);
            }
        }

        // Generate slug if not provided
        var slug = request.SlugUrl ?? GenerateSlug(request.Name);

        // Check if slug is unique for this user
        var slugExists = await _unitOfWork.Repository<Domain.Entities.Events.MeetlrEvent>().GetQueryable()
            .AnyAsync(e => e.UserId == userId && e.Slug == slug, cancellationToken);

        if (slugExists)
        {
            slug = $"{slug}-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }

        // Create event type (use UserSettings defaults if available, otherwise use sensible defaults)
        var eventType = new Domain.Entities.Events.MeetlrEvent
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name,
            Description = request.Description,
            MeetingLocationType = request.MeetingLocationType,
            LocationDetails = request.LocationDetails,
            DurationMinutes = request.DurationMinutes,
            SlotIntervalMinutes = request.SlotIntervalMinutes,
            BufferTimeBeforeMinutes = request.BufferTimeBeforeMinutes,
            BufferTimeAfterMinutes = request.BufferTimeAfterMinutes,
            Color = request.Color,
            Slug = slug,
            MinBookingNoticeMinutes = request.MinBookingNoticeMinutes,
            MaxBookingDaysInFuture = request.MaxBookingDaysInFuture,
            RequiresPayment = request.RequiresPayment,
            Fee = request.Fee,
            Currency = request.Currency,
            PaymentProviderType = request.PaymentProviderType,
            AvailabilityScheduleId = request.AvailabilityScheduleId ?? throw new InvalidOperationException("AvailabilityScheduleId is required"),
            MeetingType = request.MeetingType,
            MaxAttendeesPerSlot = request.MaxAttendeesPerSlot,
            AllowsRecurring = request.AllowsRecurring,
            MaxRecurringOccurrences = request.MaxRecurringOccurrences,
            Status = MeetlrEventStatus.Active,
            CreatedAt = DateTime.UtcNow,
            NotifyViaEmail = request.NotifyViaEmail,
            NotifyViaSms = request.NotifyViaSms,
            NotifyViaWhatsApp = request.NotifyViaWhatsApp,

            ReminderHoursBefore = user.UserSettings?.DefaultReminderHours ?? 24,
            SendReminderEmail = user.UserSettings?.DefaultSendReminderEmail ?? true,

            SendFollowUpEmail = user.UserSettings?.FollowUpEnabled ?? false,
            FollowUpHoursAfter = user.UserSettings?.FollowUpHoursAfter ?? 24,
        };

        _unitOfWork.Repository<Domain.Entities.Events.MeetlrEvent>().Add(eventType);

        // Add custom questions
        if (request.Questions != null && request.Questions.Any())
        {
            foreach (var questionRequest in request.Questions)
            {
                var question = new MeetlrEventQuestion
                {
                    Id = Guid.NewGuid(),
                    MeetlrEventId = eventType.Id,
                    QuestionText = questionRequest.QuestionText,
                    Type = Enum.Parse<QuestionType>(questionRequest.Type),
                    IsRequired = questionRequest.IsRequired,
                    DisplayOrder = questionRequest.DisplayOrder,
                    Options = questionRequest.Options,
                    CreatedAt = DateTime.UtcNow
                };

                _unitOfWork.Repository<MeetlrEventQuestion>().Add(question);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Seed default email templates for this event (if Notifications module is available)
        if (_emailTemplateSeeder != null)
        {
            await _emailTemplateSeeder.SeedDefaultTemplatesForEventAsync(
                eventType.Id,
                user.TenantId,
                cancellationToken);
        }

        // Log audit
        await _auditService.LogAsync(
            AuditEntityType.MeetlrEvent,
            eventType.Id.ToString(),
            AuditAction.Create,
            null,
            eventType,
            cancellationToken);

        // Build booking URL
        var bookingUrl = _urlsSettings.BuildBookingUrl(user.MeetlrUsername ?? string.Empty, slug);

        return new CreateMeetlrEventCommandResponse
        {
            Id = eventType.Id,
            Name = eventType.Name,
            Description = eventType.Description,
            DurationMinutes = eventType.DurationMinutes,
            SlugUrl = eventType.Slug,
            BookingUrl = bookingUrl,
            RequiresPayment = eventType.RequiresPayment,
            Fee = eventType.Fee,
            Currency = eventType.Currency,
            CreatedAt = eventType.CreatedAt
        };
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLower()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace("\"", "");

        // Remove special characters, keep only alphanumeric and hyphens
        slug = new string(slug.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());

        // Remove consecutive hyphens
        while (slug.Contains("--"))
        {
            slug = slug.Replace("--", "-");
        }

        return slug.Trim('-');
    }
}
