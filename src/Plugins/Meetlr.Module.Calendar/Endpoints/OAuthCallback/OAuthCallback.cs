using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Common.Settings;
using Meetlr.Application.Plugins;
using Meetlr.Domain.Entities.Plugins;
using Meetlr.Module.Calendar.Application.Commands.ConnectCalendar;
using Meetlr.Module.Calendar.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using CalendarProvider = Meetlr.Module.Calendar.Domain.Enums.CalendarProvider;
using Enums_CalendarProvider = Meetlr.Module.Calendar.Domain.Enums.CalendarProvider;

namespace Meetlr.Module.Calendar.Endpoints.OAuthCallback;

/// <summary>
/// OAuth callback endpoint for calendar providers (Google, Microsoft).
/// This endpoint must be public (no auth) because it receives redirects from OAuth providers.
/// Also handles plugin OAuth connections (Google Meet, Microsoft Teams) using the same callback URL.
/// The connection type is determined by the state parameter format:
/// - Calendar: "Provider:ScheduleId:RedirectUri" (e.g., "Google:guid:http://...")
/// - Plugin: "plugin:PluginId:UserId:RedirectUri" (e.g., "plugin:googlemeet:guid:http://...")
/// </summary>
[ApiController]
[Route("api/calendar")]
public class OAuthCallback : ControllerBase
{
    private readonly IEnumerable<ICalendarProviderService> _calendarProviders;
    private readonly IEnumerable<IPlugin> _plugins;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ApplicationUrlsSettings _applicationUrls;

    public OAuthCallback(
        IEnumerable<ICalendarProviderService> calendarProviders,
        IEnumerable<IPlugin> plugins,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        IOptions<ApplicationUrlsSettings> applicationUrls)
    {
        _calendarProviders = calendarProviders;
        _plugins = plugins;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _applicationUrls = applicationUrls.Value;
    }

    [AllowAnonymous]
    [HttpGet("callback")]
    public async Task<IActionResult> Handle([FromQuery] string code, [FromQuery] string state)
    {
        // Log for debugging
        Console.WriteLine($"[OAuth Callback] Received state: {state}");
        Console.WriteLine($"[OAuth Callback] Code length: {code?.Length ?? 0}");

        if (string.IsNullOrEmpty(state) || !state.Contains(':'))
        {
            Console.WriteLine($"[OAuth Callback] Error: Invalid state parameter");
            return BadRequest(new { error = "Invalid state parameter" });
        }

        // Check if this is a plugin connection (state starts with "plugin:")
        if (state.StartsWith("plugin:", StringComparison.OrdinalIgnoreCase))
        {
            return await HandlePluginCallback(code, state);
        }

        // Otherwise, handle as calendar connection
        return await HandleCalendarCallback(code, state);
    }

    /// <summary>
    /// Handle plugin OAuth callback (Google Meet, Microsoft Teams)
    /// State format: "plugin:PluginId:UserId:ReturnUrl"
    /// </summary>
    private async Task<IActionResult> HandlePluginCallback(string code, string state)
    {
        Console.WriteLine($"[Plugin Callback] Processing plugin OAuth callback");

        // Parse state: "plugin:PluginId:UserId:ReturnUrl"
        var parts = state.Split(':', 4);
        if (parts.Length < 3)
        {
            return BadRequest(new { error = "Invalid plugin state parameter format" });
        }

        var pluginId = parts[1];
        if (!Guid.TryParse(parts[2], out var userId))
        {
            return BadRequest(new { error = "Invalid user ID in plugin state parameter" });
        }

        // The OAuth callback redirect URI must match what was used in the auth request
        // This is always the frontend callback URL that's registered in Google/Microsoft
        var oauthRedirectUri = _applicationUrls.BuildCalendarCallbackUrl();

        Console.WriteLine($"[Plugin Callback] PluginId: {pluginId}, UserId: {userId}");

        // Find the plugin
        var plugin = _plugins.FirstOrDefault(p => p.PluginId.Equals(pluginId, StringComparison.OrdinalIgnoreCase));
        if (plugin == null)
        {
            return BadRequest(new { error = $"Plugin {pluginId} not found" });
        }

        try
        {
            // Complete the plugin connection using the OAuth redirect URI
            var success = await plugin.CompleteConnectAsync(code, oauthRedirectUri, userId);

            if (!success)
            {
                return BadRequest(new { error = "Failed to connect plugin" });
            }

            // Get connection status for email
            var connectionStatus = await plugin.GetConnectionStatusAsync(userId);

            // Update the installed plugin record
            var installedPlugin = await _unitOfWork.Repository<UserInstalledPlugin>()
                .GetQueryable()
                .FirstOrDefaultAsync(p => p.UserId == userId &&
                                          p.PluginId == pluginId &&
                                          !p.IsDeleted);

            if (installedPlugin != null)
            {
                installedPlugin.IsConnected = true;
                installedPlugin.ConnectedAt = DateTime.UtcNow;
                installedPlugin.ConnectionStatus = "connected";
                installedPlugin.ErrorMessage = null;
                installedPlugin.ProviderEmail = connectionStatus?.Email;
                await _unitOfWork.SaveChangesAsync();
            }

            return Ok(new OAuthCallbackResponse
            {
                Success = true,
                Message = $"{plugin.PluginName} connected successfully",
                Provider = pluginId,
                Email = connectionStatus?.Email,
                IsPlugin = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Plugin Callback] Error: {ex.Message}");
            return BadRequest(new { error = $"Failed to connect plugin: {ex.Message}" });
        }
    }

    /// <summary>
    /// Handle calendar OAuth callback
    /// State format: "Provider:ScheduleId:RedirectUri"
    /// </summary>
    private async Task<IActionResult> HandleCalendarCallback(string code, string state)
    {
        Console.WriteLine($"[Calendar Callback] Processing calendar OAuth callback");

        // Split on colons: Provider:ScheduleId:RedirectUri
        var parts = state.Split(':', 3);
        Console.WriteLine($"[Calendar Callback] State parts: {string.Join(" | ", parts)}");

        if (parts.Length < 2)
        {
            Console.WriteLine($"[Calendar Callback] Error: Invalid state parameter format");
            return BadRequest(new { error = "Invalid state parameter format" });
        }

        var provider = parts[0];

        if (!Guid.TryParse(parts[1], out var scheduleId))
        {
            Console.WriteLine($"[Calendar Callback] Error: Invalid schedule ID '{parts[1]}'");
            return BadRequest(new { error = "Invalid schedule ID in state parameter" });
        }

        // Get redirect URI from state or use default
        var redirectUri = parts.Length > 2 ? parts[2] : _applicationUrls.BuildCalendarCallbackUrl();

        if (!Enum.TryParse<Enums_CalendarProvider>(provider, true, out var calendarProvider))
        {
            return BadRequest(new { error = "Invalid calendar provider" });
        }

        try
        {
            // Get the calendar provider service
            var providerService = _calendarProviders.FirstOrDefault(p => p.Provider == calendarProvider);
            if (providerService == null)
            {
                return BadRequest(new { error = $"Calendar provider {calendarProvider} is not supported" });
            }

            // Exchange authorization code for tokens
            var tokenResponse = await providerService.ExchangeCodeForTokenAsync(code, redirectUri);

            // Get user email from the provider
            var userEmail = await providerService.GetUserEmailAsync(tokenResponse.AccessToken);

            // Use CQRS command to save tokens - linked to schedule
            var command = new ConnectCalendarCommand
            {
                ScheduleId = scheduleId,
                Provider = calendarProvider,
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken,
                TokenExpiresAt = tokenResponse.ExpiresAt,
                ProviderEmail = userEmail
            };

            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                return BadRequest(new { error = result.Message });
            }

            return Ok(new OAuthCallbackResponse
            {
                Success = true,
                Message = result.Message,
                Provider = provider
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = $"Failed to connect calendar: {ex.Message}" });
        }
    }
}
