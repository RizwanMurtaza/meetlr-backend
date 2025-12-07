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
using Microsoft.Extensions.Options;

namespace Meetlr.Plugins.MeetingTypes.Providers.Zoom;

/// <summary>
/// Zoom provider - requires separate OAuth connection
/// </summary>
public class ZoomProvider : IMeetingTypesPlugin
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ZoomSettings _settings;
    private readonly ApplicationUrlsSettings _applicationUrls;

    public ZoomProvider(
        IUnitOfWork unitOfWork,
        IHttpClientFactory httpClientFactory,
        IOptions<ZoomSettings> settings,
        IOptions<ApplicationUrlsSettings> applicationUrls)
    {
        _unitOfWork = unitOfWork;
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
        _applicationUrls = applicationUrls.Value;
    }

    // IPlugin properties
    public PluginCategory Category => PluginCategory.VideoConferencing;
    public string PluginId => "zoom";
    public string PluginName => "Zoom";
    public string Description => "Create Zoom video meetings for your bookings";
    public string Version => "1.0.0";
    public string? Author => "Meetlr Team";
    public string? IconUrl => "/images/plugins/zoom.svg";
    public bool IsEnabled => true; // Always enabled, but connection requires OAuth credentials
    private bool IsConfigured => !string.IsNullOrEmpty(_settings.ClientId);
    public bool RequiresConnection => true;

    // IMeetingTypesPlugin property
    public MeetingLocationType LocationType => MeetingLocationType.Zoom;

    public async Task<bool> IsAvailableForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var account = await GetZoomAccountAsync(userId, cancellationToken);
        return account != null;
    }

    public async Task<PluginConnectionStatus?> GetConnectionStatusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var account = await GetZoomAccountAsync(userId, cancellationToken);

        if (account == null)
        {
            return null;
        }

        var needsReconnect = account.TokenExpiresAt.HasValue &&
                            account.TokenExpiresAt.Value < DateTime.UtcNow &&
                            string.IsNullOrEmpty(account.RefreshToken);

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
        var account = await GetZoomAccountAsync(request.UserId, cancellationToken);

        if (account == null)
        {
            throw new InvalidOperationException("Zoom is not connected. Please connect your Zoom account first.");
        }

        // Refresh token if needed
        var accessToken = await EnsureValidTokenAsync(account, cancellationToken);

        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Create meeting via Zoom API
        var meetingRequest = new
        {
            topic = request.Title,
            type = 2, // Scheduled meeting
            start_time = request.StartTime.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            duration = request.DurationMinutes,
            timezone = "UTC",
            settings = new
            {
                host_video = true,
                participant_video = true,
                join_before_host = true,
                mute_upon_entry = false,
                waiting_room = false
            }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(meetingRequest),
            Encoding.UTF8,
            "application/json");

        var response = await httpClient.PostAsync(
            $"{_settings.ApiBaseUrl}/users/me/meetings",
            content,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Failed to create Zoom meeting: {error}");
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;

        var joinUrl = root.GetProperty("join_url").GetString()
            ?? throw new InvalidOperationException("Zoom meeting created but no join URL returned");

        var meetingId = root.GetProperty("id").GetInt64().ToString();

        string? password = null;
        if (root.TryGetProperty("password", out var passwordElement))
        {
            password = passwordElement.GetString();
        }

        string? hostUrl = null;
        if (root.TryGetProperty("start_url", out var startUrlElement))
        {
            hostUrl = startUrlElement.GetString();
        }

        return new VideoMeetingResult
        {
            JoinUrl = joinUrl,
            MeetingId = meetingId,
            HostUrl = hostUrl,
            Password = password
        };
    }

    public async Task<bool> DeleteMeetingAsync(string meetingId, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var account = await GetZoomAccountAsync(userId, cancellationToken);

            if (account == null)
            {
                return false;
            }

            var accessToken = await EnsureValidTokenAsync(account, cancellationToken);

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.DeleteAsync(
                $"{_settings.ApiBaseUrl}/meetings/{meetingId}",
                cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public Task<string?> GenerateConnectUrlAsync(Guid userId, string returnUrl, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            return Task.FromResult<string?>(null);
        }

        // Use the frontend callback URL (which should be registered in Zoom OAuth app)
        var callbackUrl = _applicationUrls.BuildCalendarCallbackUrl();
        var state = $"plugin:{PluginId}:{userId}:{returnUrl}";
        var url = $"{_settings.AuthorizationUrl}?" +
               $"response_type=code" +
               $"&client_id={_settings.ClientId}" +
               $"&redirect_uri={Uri.EscapeDataString(callbackUrl)}" +
               $"&state={Uri.EscapeDataString(state)}";

        return Task.FromResult<string?>(url);
    }

    public async Task<bool> CompleteConnectAsync(string code, string redirectUri, Guid userId, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient();

        // Create Basic auth header
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        var parameters = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", redirectUri }
        };

        var content = new FormUrlEncodedContent(parameters);
        var response = await httpClient.PostAsync(_settings.TokenUrl, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;

        var accessToken = root.GetProperty("access_token").GetString()!;
        var refreshToken = root.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
        var expiresIn = root.GetProperty("expires_in").GetInt32();

        // Get user info
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var userResponse = await httpClient.GetAsync($"{_settings.ApiBaseUrl}/users/me", cancellationToken);
        var userBody = await userResponse.Content.ReadAsStringAsync(cancellationToken);
        using var userDoc = JsonDocument.Parse(userBody);
        var userRoot = userDoc.RootElement;

        var email = userRoot.TryGetProperty("email", out var emailElement) ? emailElement.GetString() : null;
        var zoomUserId = userRoot.GetProperty("id").GetString()!;

        // Save or update account
        var existingAccount = await GetZoomAccountAsync(userId, cancellationToken);

        if (existingAccount != null)
        {
            existingAccount.AccessToken = accessToken;
            existingAccount.RefreshToken = refreshToken;
            existingAccount.TokenExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn);
            existingAccount.Email = email;
            existingAccount.ProviderAccountId = zoomUserId;
        }
        else
        {
            var newAccount = new VideoConferencingAccount
            {
                UserId = userId,
                Provider = PluginId,
                ProviderAccountId = zoomUserId,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn),
                Email = email,
                ConnectedAt = DateTime.UtcNow
            };

            _unitOfWork.Repository<VideoConferencingAccount>().Add(newAccount);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Disconnect Zoom account
    /// </summary>
    public async Task<bool> DisconnectAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var account = await GetZoomAccountAsync(userId, cancellationToken);

        if (account == null)
        {
            return false;
        }

        _unitOfWork.Repository<VideoConferencingAccount>().Delete(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public Task<PluginHealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        var status = new PluginHealthStatus
        {
            IsHealthy = IsConfigured,
            Status = IsConfigured ? "Healthy" : "Not Configured",
            Message = IsConfigured ? "Zoom integration is configured" : "Zoom OAuth credentials not configured. Contact your administrator."
        };

        return Task.FromResult(status);
    }

    private async Task<VideoConferencingAccount?> GetZoomAccountAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<VideoConferencingAccount>()
            .GetQueryable()
            .FirstOrDefaultAsync(
                a => a.UserId == userId &&
                     a.Provider == PluginId &&
                     !a.IsDeleted,
                cancellationToken);
    }

    private async Task<string> EnsureValidTokenAsync(VideoConferencingAccount account, CancellationToken cancellationToken)
    {
        // Check if token is expired or about to expire
        if (account.TokenExpiresAt.HasValue &&
            account.TokenExpiresAt.Value <= DateTime.UtcNow.AddMinutes(5))
        {
            if (string.IsNullOrEmpty(account.RefreshToken))
            {
                throw new InvalidOperationException("Zoom token expired and no refresh token available. Please reconnect your Zoom account.");
            }

            // Refresh the token
            var httpClient = _httpClientFactory.CreateClient();
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var parameters = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", account.RefreshToken }
            };

            var content = new FormUrlEncodedContent(parameters);
            var response = await httpClient.PostAsync(_settings.TokenUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("Failed to refresh Zoom token. Please reconnect your Zoom account.");
            }

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            account.AccessToken = root.GetProperty("access_token").GetString();
            if (root.TryGetProperty("refresh_token", out var rt))
            {
                account.RefreshToken = rt.GetString();
            }
            account.TokenExpiresAt = DateTime.UtcNow.AddSeconds(root.GetProperty("expires_in").GetInt32());

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return account.AccessToken!;
    }
}
