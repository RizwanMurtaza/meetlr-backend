namespace Meetlr.Domain.Enums;

/// <summary>
/// Categories of plugins supported by the system
/// </summary>
public enum PluginCategory
{
    /// <summary>
    /// Payment processing plugins (Stripe, PayPal, Square, etc.)
    /// </summary>
    Payment = 1,
    
    /// <summary>
    /// Calendar integration plugins (Google Calendar, Outlook, Apple Calendar, etc.)
    /// </summary>
    Calendar = 2,
    
    /// <summary>
    /// Video conferencing plugins (Zoom, Microsoft Teams, Google Meet, etc.)
    /// </summary>
    VideoConferencing = 3,
    
    /// <summary>
    /// Communication/messaging plugins (Slack, Discord, Telegram, etc.)
    /// </summary>
    Communication = 4,
    
    /// <summary>
    /// Analytics and reporting plugins (Google Analytics, Mixpanel, etc.)
    /// </summary>
    Analytics = 5,
    
    /// <summary>
    /// CRM integration plugins (Salesforce, HubSpot, Pipedrive, etc.)
    /// </summary>
    CRM = 6,
    
    /// <summary>
    /// Email marketing plugins (Mailchimp, SendGrid, etc.)
    /// </summary>
    EmailMarketing = 7,
    
    /// <summary>
    /// Automation and workflow plugins (Zapier, Make, n8n, etc.)
    /// </summary>
    Automation = 8,
    
    /// <summary>
    /// Storage and file sharing plugins (Dropbox, Google Drive, OneDrive, etc.)
    /// </summary>
    Storage = 9,
    
    /// <summary>
    /// Custom/third-party plugins
    /// </summary>
    Custom = 99
}
