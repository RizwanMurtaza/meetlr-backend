using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
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
using GoogleCalendarService = Google.Apis.Calendar.v3.CalendarService;

namespace Meetlr.Plugins.MeetingTypes.Providers.GoogleMeet;

/// <summary>
/// Google Meet provider - stores its own OAuth tokens in UserInstalledPlugin
/// </summary>
public class GoogleMeetProvider : IMeetingTypesPlugin
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly GoogleOAuthService _googleOAuthService;
    private readonly ApplicationUrlsSettings _applicationUrls;

    public GoogleMeetProvider(
        IUnitOfWork unitOfWork,
        GoogleOAuthService googleOAuthService,
        IOptions<ApplicationUrlsSettings> applicationUrls)
    {
        _unitOfWork = unitOfWork;
        _googleOAuthService = googleOAuthService;
        _applicationUrls = applicationUrls.Value;
    }

    // IPlugin properties
    public PluginCategory Category => PluginCategory.VideoConferencing;
    public string PluginId => "googlemeet";
    public string PluginName => "Google Meet";
    public string Description => "Create Google Meet video meetings for your bookings";
    public string Version => "1.0.0";
    public string? Author => "Meetlr Team";
    public string? IconUrl => "/images/plugins/google-meet.svg";
    public bool IsEnabled => true;
    public bool RequiresConnection => true;

    // IMeetingTypesPlugin property
    public MeetingLocationType LocationType => MeetingLocationType.GoogleMeet;

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
        // Generate Google OAuth URL for this plugin
        // Use the FRONTEND callback URL (which is registered in Google Cloud Console)
        // The frontend will then call the backend API with the code
        // State format: plugin:pluginId:userId:returnUrl - this identifies it as a plugin connection
        var callbackUrl = _applicationUrls.BuildCalendarCallbackUrl();
        var state = $"plugin:{PluginId}:{userId}:{returnUrl}";
        var authUrl = _googleOAuthService.GetAuthorizationUrl(callbackUrl, state);
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
            var tokenResponse = await _googleOAuthService.ExchangeCodeForTokenAsync(code, redirectUri);

            // Get user info from Google
            var userInfo = await _googleOAuthService.GetUserInfoAsync(tokenResponse.AccessToken);

            // Store tokens in the installed plugin record
            installedPlugin.AccessToken = tokenResponse.AccessToken;
            installedPlugin.RefreshToken = tokenResponse.RefreshToken;
            installedPlugin.TokenExpiresAt = tokenResponse.IssuedUtc.AddSeconds(tokenResponse.ExpiresInSeconds ?? 3600);
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
            Message = "Google Meet is available"
        });
    }

    public async Task<VideoMeetingResult> CreateMeetingAsync(CreateMeetingRequest request, CancellationToken cancellationToken = default)
    {
        var installedPlugin = await GetInstalledPluginAsync(request.UserId, cancellationToken);

        if (installedPlugin == null || !installedPlugin.IsConnected)
        {
            throw new InvalidOperationException("Google Meet is not connected. Please connect your Google account first.");
        }

        // Refresh token if needed
        var accessToken = await EnsureValidTokenAsync(installedPlugin, cancellationToken);

        var credential = GoogleCredential.FromAccessToken(accessToken);
        var service = new GoogleCalendarService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential
        });

        var newEvent = new Event
        {
            Summary = request.Title,
            Start = new EventDateTime
            {
                DateTimeDateTimeOffset = new DateTimeOffset(request.StartTime, TimeSpan.Zero)
            },
            End = new EventDateTime
            {
                DateTimeDateTimeOffset = new DateTimeOffset(request.StartTime.AddMinutes(request.DurationMinutes), TimeSpan.Zero)
            },
            ConferenceData = new ConferenceData
            {
                CreateRequest = new CreateConferenceRequest
                {
                    RequestId = Guid.NewGuid().ToString(),
                    ConferenceSolutionKey = new ConferenceSolutionKey
                    {
                        Type = "hangoutsMeet"
                    }
                }
            }
        };

        if (!string.IsNullOrEmpty(request.AttendeeEmail))
        {
            newEvent.Attendees = new List<EventAttendee>
            {
                new EventAttendee
                {
                    Email = request.AttendeeEmail,
                    DisplayName = request.AttendeeName
                }
            };
        }

        var insertRequest = service.Events.Insert(newEvent, "primary");
        insertRequest.ConferenceDataVersion = 1; // Required for creating Google Meet
        insertRequest.SendUpdates = Google.Apis.Calendar.v3.EventsResource.InsertRequest.SendUpdatesEnum.None;

        var createdEvent = await insertRequest.ExecuteAsync(cancellationToken);

        var meetLink = createdEvent.ConferenceData?.EntryPoints?
            .FirstOrDefault(e => e.EntryPointType == "video")?.Uri;

        if (string.IsNullOrEmpty(meetLink))
        {
            throw new InvalidOperationException("Failed to create Google Meet link. The conference was not created.");
        }

        return new VideoMeetingResult
        {
            JoinUrl = meetLink,
            MeetingId = createdEvent.Id,
            DialIn = createdEvent.ConferenceData?.EntryPoints?
                .FirstOrDefault(e => e.EntryPointType == "phone")?.Uri
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

            var credential = GoogleCredential.FromAccessToken(accessToken);
            var service = new GoogleCalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential
            });

            await service.Events.Delete("primary", meetingId).ExecuteAsync(cancellationToken);
            return true;
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
                throw new InvalidOperationException("Google token expired and no refresh token available. Please reconnect your Google account.");
            }

            // Refresh the token
            var tokenResponse = await _googleOAuthService.RefreshTokenAsync(installedPlugin.RefreshToken);

            installedPlugin.AccessToken = tokenResponse.AccessToken;
            if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
            {
                installedPlugin.RefreshToken = tokenResponse.RefreshToken;
            }
            installedPlugin.TokenExpiresAt = tokenResponse.IssuedUtc.AddSeconds(tokenResponse.ExpiresInSeconds ?? 3600);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return installedPlugin.AccessToken!;
    }
}
