using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Common.Settings;
using Meetlr.Application.Plugins;
using Meetlr.Application.Plugins.MeetingTypes;
using Meetlr.Application.Plugins.MeetingTypes.Models;
using Meetlr.Application.Plugins.Models;
using Meetlr.Domain.Entities.VideoConferencing;
using Meetlr.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Meetlr.Plugins.MeetingTypes.Providers.Slack;

/// <summary>
/// Slack Huddle provider - requires OAuth connection
/// Creates Slack Huddles for quick audio/video meetings
/// </summary>
public class SlackHuddleProvider : IMeetingTypesPlugin
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SlackSettings _settings;
    private readonly ApplicationUrlsSettings _applicationUrls;
    private readonly ILogger<SlackHuddleProvider> _logger;

    public SlackHuddleProvider(
        IUnitOfWork unitOfWork,
        IHttpClientFactory httpClientFactory,
        IOptions<SlackSettings> settings,
        IOptions<ApplicationUrlsSettings> applicationUrls,
        ILogger<SlackHuddleProvider> logger)
    {
        _unitOfWork = unitOfWork;
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
        _applicationUrls = applicationUrls.Value;
        _logger = logger;
    }

    // IPlugin properties
    public PluginCategory Category => PluginCategory.VideoConferencing;
    public string PluginId => "slackhuddle";
    public string PluginName => "Slack Huddle";
    public string Description => "Create Slack Huddles for quick audio/video meetings";
    public string Version => "1.0.0";
    public string? Author => "Meetlr Team";
    public string? IconUrl => "/images/plugins/slack.svg";
    public bool IsEnabled => true; // Always enabled, but connection requires OAuth credentials
    private bool IsConfigured => !string.IsNullOrEmpty(_settings.ClientId);
    public bool RequiresConnection => true;

    // IMeetingTypesPlugin property
    public MeetingLocationType LocationType => MeetingLocationType.SlackHuddle;

    public async Task<bool> IsAvailableForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var account = await GetSlackAccountAsync(userId, cancellationToken);
        return account != null;
    }

    public async Task<PluginConnectionStatus?> GetConnectionStatusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var account = await GetSlackAccountAsync(userId, cancellationToken);

        if (account == null)
        {
            return null;
        }

        // Slack tokens don't expire, but we track if there's an issue
        var needsReconnect = account.TokenExpiresAt.HasValue &&
                            account.TokenExpiresAt.Value < DateTime.UtcNow;

        return new PluginConnectionStatus
        {
            IsConnected = true,
            Email = account.Email,
            ConnectedAt = account.ConnectedAt,
            NeedsReconnect = needsReconnect
        };
    }

    public async Task<VideoMeetingResult> CreateMeetingAsync(CreateMeetingRequest request, CancellationToken cancellationToken = default)
    {
        var account = await GetSlackAccountAsync(request.UserId, cancellationToken);

        if (account == null)
        {
            throw new InvalidOperationException("Slack is not connected. Please connect your Slack account first.");
        }

        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", account.AccessToken);

        // Create a Slack Call (Huddle) via the Calls API
        // https://api.slack.com/methods/calls.add
        var callRequest = new Dictionary<string, string>
        {
            { "external_unique_id", $"{request.BookingId}_{DateTime.UtcNow.Ticks}" },
            { "join_url", $"https://app.slack.com/huddle/{account.ProviderAccountId}" }, // Will be replaced with actual URL
            { "title", request.Title },
            { "date_start", ((DateTimeOffset)request.StartTime).ToUnixTimeSeconds().ToString() }
        };

        var content = new FormUrlEncodedContent(callRequest);
        var response = await httpClient.PostAsync(
            $"{_settings.ApiBaseUrl}/calls.add",
            content,
            cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        _logger.LogDebug("Slack calls.add response: {Response}", responseBody);

        using var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;

        if (!root.TryGetProperty("ok", out var okElement) || !okElement.GetBoolean())
        {
            var error = root.TryGetProperty("error", out var errorElement)
                ? errorElement.GetString()
                : "Unknown error";

            _logger.LogWarning("Failed to create Slack call: {Error}", error);

            // Fallback: Generate a workspace huddle URL
            // Users can start a huddle directly in their workspace
            var workspaceUrl = $"slack://huddle?team={account.ProviderAccountId}";

            return new VideoMeetingResult
            {
                JoinUrl = workspaceUrl,
                MeetingId = $"huddle_{request.BookingId}",
                HostUrl = workspaceUrl
            };
        }

        var call = root.GetProperty("call");
        var callId = call.GetProperty("id").GetString()!;
        var joinUrl = call.TryGetProperty("join_url", out var joinUrlElement)
            ? joinUrlElement.GetString()
            : $"slack://huddle?call={callId}";

        return new VideoMeetingResult
        {
            JoinUrl = joinUrl ?? $"slack://huddle?call={callId}",
            MeetingId = callId,
            HostUrl = joinUrl
        };
    }

    public async Task<bool> DeleteMeetingAsync(string meetingId, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var account = await GetSlackAccountAsync(userId, cancellationToken);

            if (account == null)
            {
                return false;
            }

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", account.AccessToken);

            // End the call via Slack API
            var endRequest = new Dictionary<string, string>
            {
                { "id", meetingId }
            };

            var content = new FormUrlEncodedContent(endRequest);
            var response = await httpClient.PostAsync(
                $"{_settings.ApiBaseUrl}/calls.end",
                content,
                cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            return root.TryGetProperty("ok", out var okElement) && okElement.GetBoolean();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete Slack meeting {MeetingId}", meetingId);
            return false;
        }
    }

    public Task<string?> GenerateConnectUrlAsync(Guid userId, string returnUrl, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            return Task.FromResult<string?>(null);
        }

        // Use the frontend callback URL (which should be registered in Slack OAuth app)
        var callbackUrl = _applicationUrls.BuildCalendarCallbackUrl();
        var state = $"plugin:{PluginId}:{userId}:{returnUrl}";
        var url = $"{_settings.AuthorizationUrl}?" +
               $"client_id={_settings.ClientId}" +
               $"&scope={Uri.EscapeDataString(_settings.Scopes)}" +
               $"&redirect_uri={Uri.EscapeDataString(callbackUrl)}" +
               $"&state={Uri.EscapeDataString(state)}" +
               $"&user_scope={Uri.EscapeDataString(_settings.Scopes)}";

        return Task.FromResult<string?>(url);
    }

    public async Task<bool> CompleteConnectAsync(string code, string redirectUri, Guid userId, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var parameters = new Dictionary<string, string>
        {
            { "client_id", _settings.ClientId },
            { "client_secret", _settings.ClientSecret },
            { "code", code },
            { "redirect_uri", redirectUri }
        };

        var content = new FormUrlEncodedContent(parameters);
        var response = await httpClient.PostAsync(_settings.TokenUrl, content, cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        _logger.LogDebug("Slack OAuth response: {Response}", responseBody);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Slack OAuth failed: {Response}", responseBody);
            return false;
        }

        using var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;

        if (!root.TryGetProperty("ok", out var okElement) || !okElement.GetBoolean())
        {
            var error = root.TryGetProperty("error", out var errorElement)
                ? errorElement.GetString()
                : "Unknown error";
            _logger.LogWarning("Slack OAuth error: {Error}", error);
            return false;
        }

        // Get authed_user for user token
        string? accessToken = null;
        string? slackUserId = null;

        if (root.TryGetProperty("authed_user", out var authedUser))
        {
            accessToken = authedUser.TryGetProperty("access_token", out var atElement)
                ? atElement.GetString()
                : null;
            slackUserId = authedUser.TryGetProperty("id", out var idElement)
                ? idElement.GetString()
                : null;
        }

        // Fallback to app-level token if user token not available
        if (string.IsNullOrEmpty(accessToken) && root.TryGetProperty("access_token", out var appToken))
        {
            accessToken = appToken.GetString();
        }

        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogWarning("No access token in Slack OAuth response");
            return false;
        }

        // Get team ID
        var teamId = root.TryGetProperty("team", out var team) && team.TryGetProperty("id", out var teamIdElement)
            ? teamIdElement.GetString()
            : null;

        // Get user info to get email
        string? email = null;
        if (!string.IsNullOrEmpty(slackUserId))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var userResponse = await httpClient.GetAsync(
                $"{_settings.ApiBaseUrl}/users.info?user={slackUserId}",
                cancellationToken);

            var userBody = await userResponse.Content.ReadAsStringAsync(cancellationToken);
            using var userDoc = JsonDocument.Parse(userBody);
            var userRoot = userDoc.RootElement;

            if (userRoot.TryGetProperty("ok", out var userOk) && userOk.GetBoolean() &&
                userRoot.TryGetProperty("user", out var user))
            {
                if (user.TryGetProperty("profile", out var profile) &&
                    profile.TryGetProperty("email", out var emailElement))
                {
                    email = emailElement.GetString();
                }
            }
        }

        // Save or update account
        var existingAccount = await GetSlackAccountAsync(userId, cancellationToken);

        if (existingAccount != null)
        {
            existingAccount.AccessToken = accessToken;
            existingAccount.RefreshToken = null; // Slack user tokens don't have refresh tokens
            existingAccount.TokenExpiresAt = null; // Slack user tokens don't expire
            existingAccount.Email = email;
            existingAccount.ProviderAccountId = slackUserId ?? teamId ?? "unknown";
        }
        else
        {
            var newAccount = new VideoConferencingAccount
            {
                UserId = userId,
                Provider = PluginId,
                ProviderAccountId = slackUserId ?? teamId ?? "unknown",
                AccessToken = accessToken,
                RefreshToken = null,
                TokenExpiresAt = null,
                Email = email,
                ConnectedAt = DateTime.UtcNow
            };

            _unitOfWork.Repository<VideoConferencingAccount>().Add(newAccount);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Slack connected successfully for user {UserId}", userId);
        return true;
    }

    public async Task<bool> DisconnectAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var account = await GetSlackAccountAsync(userId, cancellationToken);

        if (account == null)
        {
            return false;
        }

        _unitOfWork.Repository<VideoConferencingAccount>().Delete(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Slack disconnected for user {UserId}", userId);
        return true;
    }

    public Task<PluginHealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        var status = new PluginHealthStatus
        {
            IsHealthy = IsConfigured,
            Status = IsConfigured ? "Healthy" : "Not Configured",
            Message = IsConfigured ? "Slack integration is configured" : "Slack OAuth credentials not configured. Contact your administrator."
        };

        return Task.FromResult(status);
    }

    private async Task<VideoConferencingAccount?> GetSlackAccountAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<VideoConferencingAccount>()
            .GetQueryable()
            .FirstOrDefaultAsync(
                a => a.UserId == userId &&
                     a.Provider == PluginId &&
                     !a.IsDeleted,
                cancellationToken);
    }
}
