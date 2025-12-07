using Meetlr.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Meetlr.Scheduler.Services.Notifications.Handlers;

/// <summary>
/// Factory for resolving the appropriate notification handler based on notification type.
/// </summary>
public interface INotificationHandlerFactory
{
    INotificationTypeHandler? GetHandler(NotificationType notificationType);
}

public class NotificationHandlerFactory : INotificationHandlerFactory
{
    private readonly IServiceProvider _serviceProvider;

    public NotificationHandlerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public INotificationTypeHandler? GetHandler(NotificationType notificationType)
    {
        return notificationType switch
        {
            NotificationType.Email => _serviceProvider.GetService<EmailNotificationHandler>(),
            NotificationType.Sms => _serviceProvider.GetService<SmsNotificationHandler>(),
            NotificationType.WhatsApp => _serviceProvider.GetService<WhatsAppNotificationHandler>(),
            NotificationType.Refund => _serviceProvider.GetService<RefundNotificationHandler>(),
            NotificationType.VideoLinkCreation => _serviceProvider.GetService<VideoLinkCreationHandler>(),
            NotificationType.VideoLinkDeletion => _serviceProvider.GetService<VideoLinkDeletionHandler>(),
            NotificationType.CalendarSync => _serviceProvider.GetService<CalendarSyncHandler>(),
            NotificationType.CalendarDeletion => _serviceProvider.GetService<CalendarDeletionHandler>(),
            NotificationType.CalendarReschedule => _serviceProvider.GetService<CalendarRescheduleHandler>(),
            NotificationType.SlotInvitationEmail => _serviceProvider.GetService<SlotInvitationEmailHandler>(),
            _ => null
        };
    }
}
