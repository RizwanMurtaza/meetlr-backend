using Meetlr.Scheduler.BackgroundServices;
using Meetlr.Scheduler.Services.Cleanup;
using Meetlr.Scheduler.Services.Notifications;
using Meetlr.Scheduler.Services.Notifications.Handlers;
using Meetlr.Scheduler.Services.Payments;
using Microsoft.Extensions.DependencyInjection;

namespace Meetlr.Scheduler;

public static class DependencyInjection
{
    public static IServiceCollection AddSchedulerServices(this IServiceCollection services)
    {
        // Register Notification Processing Services
        services.AddScoped<INotificationProcessingService, NotificationProcessingService>();
        services.AddScoped<INotificationHistoryManager, NotificationHistoryManager>();
        services.AddScoped<INotificationHandlerFactory, NotificationHandlerFactory>();

        // Register Notification Type Handlers
        services.AddScoped<EmailNotificationHandler>();
        services.AddScoped<SmsNotificationHandler>();
        services.AddScoped<WhatsAppNotificationHandler>();
        services.AddScoped<RefundNotificationHandler>();
        services.AddScoped<VideoLinkCreationHandler>();
        services.AddScoped<VideoLinkDeletionHandler>();
        services.AddScoped<CalendarSyncHandler>();
        services.AddScoped<CalendarDeletionHandler>();
        services.AddScoped<CalendarRescheduleHandler>();
        services.AddScoped<SlotInvitationEmailHandler>();

        // Register Cleanup Services
        services.AddScoped<IBookingCleanupService, BookingCleanupService>();
        services.AddScoped<IOtpCleanupService, OtpCleanupService>();
        services.AddScoped<ISlotInvitationExpirationService, SlotInvitationExpirationService>();

        // Register Payment Services
        services.AddScoped<IPaymentVerificationService, PaymentVerificationService>();

        // Register Background Services
        services.AddHostedService<NotificationBackgroundService>();

        return services;
    }
}
