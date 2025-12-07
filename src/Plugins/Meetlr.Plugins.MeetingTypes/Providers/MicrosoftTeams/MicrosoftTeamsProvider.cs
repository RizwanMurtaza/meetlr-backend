using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Common.Settings;
using Meetlr.Application.Plugins;
using Meetlr.Application.Plugins.MeetingTypes;
using Meetlr.Application.Plugins.MeetingTypes.Models;
using Meetlr.Application.Plugins.Models;
using Meetlr.Domain.Entities.Plugins;
using Meetlr.Domain.Enums;
using Meetlr.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Meetlr.Plugins.MeetingTypes.Providers.MicrosoftTeams;

/// <summary>
/// Microsoft Teams provider - stores its own OAuth tokens in UserInstalledPlugin
/// </summary>
public class MicrosoftTeamsProvider : IMeetingTypesPlugin
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly MicrosoftOAuthService _microsoftOAuthService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApplicationUrlsSettings _applicationUrls;

    private const string GraphApiBaseUrl = "https://graph.microsoft.com/v1.0";

    public MicrosoftTeamsProvider(
        IUnitOfWork unitOfWork,
        MicrosoftOAuthService microsoftOAuthService,
        IHttpClientFactory httpClientFactory,
        IOptions<ApplicationUrlsSettings> applicationUrls)
    {
        _unitOfWork = unitOfWork;
        _microsoftOAuthService = microsoftOAuthService;
        _httpClientFactory = httpClientFactory;
        _applicationUrls = applicationUrls.Value;
    }

    // IPlugin properties
    public PluginCategory Category => PluginCategory.VideoConferencing;
    public string PluginId => "microsoftteams";
    public string PluginName => "Microsoft Teams";
    public string Description => "Create Microsoft Teams video meetings for your bookings";
    public string Version => "1.0.0";
    public string? Author => "Meetlr Team";
    public string? IconUrl => "/images/plugins/microsoft-teams.svg";
    public bool IsEnabled => true;
    public bool RequiresConnection => true;

    // IMeetingTypesPlugin property
    public MeetingLocationType LocationType => MeetingLocationType.MicrosoftTeams;

    public async Task<bool> IsAvailableForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var installedPlugin = await GetInstalledPluginAsync(userId, cancellationToken);
        return installedPlugin?.IsConnected == true && !string.IsNullOrEmpty(installedPlugin.AccessToken);
    }

    public async Task<PluginConnectionStatus?> GetConnectionStatusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var installedPlugin = await GetInstalledPluginAsync(userId, cancellationToken);

        if (installedPlugin == null || !installedPlugin.IsConnected)
        {
            return null;
        }

        var needsReconnect = installedPlugin.TokenExpiresAt.HasValue &&
                            installedPlugin.TokenExpiresAt.Value < DateTime.UtcNow &&
                            string.IsNullOrEmpty(installedPlugin.RefreshToken);

        return new PluginConnectionStatus
        {
            IsConnected = installedPlugin.IsConnected,
            Email = installedPlugin.ProviderEmail,
            ConnectedAt = installedPlugin.ConnectedAt,
            NeedsReconnect = needsReconnect
        };
    }

    public Task<string?> GenerateConnectUrlAsync(Guid userId, string returnUrl, CancellationToken cancellationToken = default)
    {
        // Generate Microsoft OAuth URL for this plugin
        // Use the FRONTEND callback URL (which is registered in Microsoft Azure)
        // The frontend will then call the backend API with the code
        // State format: plugin:pluginId:userId:returnUrl - this identifies it as a plugin connection
        var callbackUrl = _applicationUrls.BuildCalendarCallbackUrl();
        var state = $"plugin:{PluginId}:{userId}:{returnUrl}";
        var authUrl = _microsoftOAuthService.GetAuthorizationUrl(callbackUrl, state);
        return Task.FromResult<string?>(authUrl);
    }

    public async Task<bool> CompleteConnectAsync(string code, string redirectUri, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var installedPlugin = await GetInstalledPluginAsync(userId, cancellationToken);
            if (installedPlugin == null)
            {
                return false;
            }

            // Exchange code for tokens using the provided redirect URI
            var tokenResponse = await _microsoftOAuthService.ExchangeCodeForTokenAsync(code, redirectUri);

            // Get user info from Microsoft Graph
            var userInfo = await _microsoftOAuthService.GetUserInfoAsync(tokenResponse.access_token);

            // Store tokens in the installed plugin record
            installedPlugin.AccessToken = tokenResponse.access_token;
            installedPlugin.RefreshToken = tokenResponse.refresh_token;
            installedPlugin.TokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.expires_in);
            installedPlugin.ProviderEmail = userInfo.Email;
            installedPlugin.ProviderId = userInfo.Id;
            installedPlugin.IsConnected = true;
            installedPlugin.ConnectedAt = DateTime.UtcNow;
            installedPlugin.ConnectionStatus = "connected";

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> DisconnectAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var installedPlugin = await GetInstalledPluginAsync(userId, cancellationToken);
        if (installedPlugin == null)
        {
            return false;
        }

        // Clear tokens - the command handler will also update IsConnected etc.
        installedPlugin.AccessToken = null;
        installedPlugin.RefreshToken = null;
        installedPlugin.TokenExpiresAt = null;
        installedPlugin.ProviderEmail = null;
        installedPlugin.ProviderId = null;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<PluginHealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new PluginHealthStatus
        {
            IsHealthy = true,
            Status = "Healthy",
            Message = "Microsoft Teams is available"
        });
    }

    public async Task<VideoMeetingResult> CreateMeetingAsync(CreateMeetingRequest request, CancellationToken cancellationToken = default)
    {
        var installedPlugin = await GetInstalledPluginAsync(request.UserId, cancellationToken);

        if (installedPlugin == null || !installedPlugin.IsConnected)
        {
            throw new InvalidOperationException("Microsoft Teams is not connected. Please connect your Microsoft account first.");
        }

        // Refresh token if needed
        var accessToken = await EnsureValidTokenAsync(installedPlugin, cancellationToken);

        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Create an online meeting using Microsoft Graph API
        var meetingRequest = new
        {
            startDateTime = request.StartTime.ToString("yyyy-MM-ddTHH:mm:ss") + "Z",
            endDateTime = request.StartTime.AddMinutes(request.DurationMinutes).ToString("yyyy-MM-ddTHH:mm:ss") + "Z",
            subject = request.Title
        };

        var content = new StringContent(
            JsonSerializer.Serialize(meetingRequest),
            Encoding.UTF8,
            "application/json");

        var response = await httpClient.PostAsync(
            $"{GraphApiBaseUrl}/me/onlineMeetings",
            content,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Failed to create Microsoft Teams meeting: {error}");
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;

        var joinUrl = root.GetProperty("joinWebUrl").GetString()
            ?? throw new InvalidOperationException("Teams meeting created but no join URL returned");

        var meetingId = root.GetProperty("id").GetString()
            ?? throw new InvalidOperationException("Teams meeting created but no meeting ID returned");

        string? dialIn = null;
        if (root.TryGetProperty("audioConferencing", out var audio) &&
            audio.TryGetProperty("tollNumber", out var tollNumber))
        {
            dialIn = tollNumber.GetString();
        }

        return new VideoMeetingResult
        {
            JoinUrl = joinUrl,
            MeetingId = meetingId,
            DialIn = dialIn
        };
    }

    public async Task<bool> DeleteMeetingAsync(string meetingId, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var installedPlugin = await GetInstalledPluginAsync(userId, cancellationToken);

            if (installedPlugin == null || !installedPlugin.IsConnected)
            {
                return false;
            }

            var accessToken = await EnsureValidTokenAsync(installedPlugin, cancellationToken);

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.DeleteAsync(
                $"{GraphApiBaseUrl}/me/onlineMeetings/{meetingId}",
                cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private async Task<UserInstalledPlugin?> GetInstalledPluginAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<UserInstalledPlugin>()
            .GetQueryable()
            .FirstOrDefaultAsync(p => p.UserId == userId &&
                                      p.PluginId == PluginId &&
                                      !p.IsDeleted, cancellationToken);
    }

    private async Task<string> EnsureValidTokenAsync(UserInstalledPlugin installedPlugin, CancellationToken cancellationToken)
    {
        // Check if token is expired or about to expire
        if (installedPlugin.TokenExpiresAt.HasValue &&
            installedPlugin.TokenExpiresAt.Value <= DateTime.UtcNow.AddMinutes(5))
        {
            if (string.IsNullOrEmpty(installedPlugin.RefreshToken))
            {
                throw new InvalidOperationException("Microsoft token expired and no refresh token available. Please reconnect your Microsoft account.");
            }

            // Refresh the token
            var tokenResponse = await _microsoftOAuthService.RefreshTokenAsync(installedPlugin.RefreshToken);

            installedPlugin.AccessToken = tokenResponse.access_token;
            if (!string.IsNullOrEmpty(tokenResponse.refresh_token))
            {
                installedPlugin.RefreshToken = tokenResponse.refresh_token;
            }
            installedPlugin.TokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.expires_in);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return installedPlugin.AccessToken!;
    }
}
