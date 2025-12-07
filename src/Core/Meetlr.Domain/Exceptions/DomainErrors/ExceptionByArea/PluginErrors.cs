using Meetlr.Domain.Exceptions.Base;
using Meetlr.Domain.Exceptions.Http;

namespace Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;

/// <summary>
/// Defines errors related to plugin management.
/// All errors use ApplicationArea.Plugins (area code: 11)
/// </summary>
public static class PluginErrors
{
    private const ApplicationArea Area = ApplicationArea.Plugins;

    public static NotFoundException PluginNotFound(string pluginId, string? customMessage = null)
    {
        var exception = new NotFoundException(Area, 1, customMessage ?? "Plugin not found");
        exception.WithDetail("PluginId", pluginId);
        return exception;
    }

    public static ConflictException PluginAlreadyInstalled(string pluginId, string? customMessage = null)
    {
        var exception = new ConflictException(Area, 2, customMessage ?? "Plugin is already installed");
        exception.WithDetail("PluginId", pluginId);
        return exception;
    }

    public static BadRequestException PluginNotAvailable(string pluginId, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 3, customMessage ?? "Plugin is not available");
        exception.WithDetail("PluginId", pluginId);
        return exception;
    }

    public static NotFoundException PluginNotInstalled(string pluginId, string? customMessage = null)
    {
        var exception = new NotFoundException(Area, 4, customMessage ?? "Plugin is not installed");
        exception.WithDetail("PluginId", pluginId);
        return exception;
    }

    public static BadRequestException PaymentPluginRequired(string? customMessage = null)
    {
        var exception = new BadRequestException(
            Area, 
            5, 
            customMessage ?? "Cannot create paid event. Please install and connect a payment provider plugin first."
        );
        exception.WithDetail("RequiredPluginCategory", "payment");
        exception.WithDetail("ErrorCode", "PAYMENT_PLUGIN_REQUIRED");
        return exception;
    }

    public static BadRequestException PluginNotEnabled(string pluginId, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 6, customMessage ?? "Plugin is not enabled");
        exception.WithDetail("PluginId", pluginId);
        return exception;
    }

    public static BadRequestException PluginNotConnected(string pluginId, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 7, customMessage ?? "Plugin is not connected. Please complete the connection process.");
        exception.WithDetail("PluginId", pluginId);
        return exception;
    }

    public static BadRequestException PluginNotConnectedOrNotInstalled(string pluginId, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 8, customMessage ?? $"Payment provider '{pluginId}' is not installed or not connected. Please install and connect it in the Integrations page before creating paid events.");
        exception.WithDetail("PluginId", pluginId);
        exception.WithDetail("RequiredAction", "InstallAndConnect");
        return exception;
    }

    public static BadRequestException PluginConnectionNotSupported(string pluginId, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 9, customMessage ?? "This plugin does not support OAuth connection.");
        exception.WithDetail("PluginId", pluginId);
        return exception;
    }
}
