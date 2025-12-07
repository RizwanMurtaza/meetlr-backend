using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Meetlr.Application.Common.Settings;
using Meetlr.Module.Calendar.Application.Interfaces;
using Meetlr.Module.Calendar.Application.Models;
using Meetlr.Module.Calendar.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using GoogleCalendarService = Google.Apis.Calendar.v3.CalendarService;

namespace Meetlr.Module.Calendar.Infrastructure.Services;

/// <summary>
/// Google Calendar provider service implementing ICalendarProviderService for OAuth operations.
/// Self-contained OAuth implementation without external dependencies.
/// </summary>
internal class GoogleCalendarProviderService : ICalendarProviderService
{
    private readonly IConfiguration _configuration;
    private readonly ExternalApisSettings _externalApisSettings;
    private readonly IHttpClientFactory _httpClientFactory;
    private const string OPENID_SCOPE = "openid";

    public GoogleCalendarProviderService(
        IConfiguration configuration,
        IOptions<ExternalApisSettings> externalApisSettings,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _externalApisSettings = externalApisSettings.Value;
        _httpClientFactory = httpClientFactory;
    }

    public CalendarProvider Provider => CalendarProvider.Google;

    public string ProviderName => "Google Calendar";

    public string GetAuthorizationUrl(string redirectUri, string state)
    {
        var flow = CreateGoogleAuthFlow();

        var authorizationUrl = flow.CreateAuthorizationCodeRequest(redirectUri);
        authorizationUrl.State = state;

        return authorizationUrl.Build().ToString();
    }

    public async Task<TokenRefreshResult> ExchangeCodeForTokenAsync(string code, string redirectUri)
    {
        var flow = CreateGoogleAuthFlow();

        var tokenResponse = await flow.ExchangeCodeForTokenAsync(
            userId: "user",
            code: code,
            redirectUri: redirectUri,
            CancellationToken.None);

        return new TokenRefreshResult
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            ExpiresAt = tokenResponse.IssuedUtc.AddSeconds(tokenResponse.ExpiresInSeconds ?? 3600)
        };
    }

    public async Task<string?> GetUserEmailAsync(string accessToken)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.GetAsync(_externalApisSettings.Google.UserInfoEndpoint);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var userInfo = JsonDocument.Parse(json);
                return userInfo.RootElement.GetProperty("email").GetString();
            }
        }
        catch
        {
            // Log error but don't fail
        }

        return null;
    }

    public async Task<TokenRefreshResult> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var flow = CreateGoogleAuthFlow();

        var tokenResponse = await flow.RefreshTokenAsync(
            userId: "user",
            refreshToken: refreshToken,
            cancellationToken);

        return new TokenRefreshResult
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            ExpiresAt = tokenResponse.IssuedUtc.AddSeconds(tokenResponse.ExpiresInSeconds ?? 3600)
        };
    }

    public async Task<List<BusyTimeSlot>> GetBusyTimesAsync(string accessToken, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var service = CreateCalendarService(accessToken);

        var request = service.Events.List("primary");
        request.TimeMinDateTimeOffset = new DateTimeOffset(startDate);
        request.TimeMaxDateTimeOffset = new DateTimeOffset(endDate);
        request.SingleEvents = true;
        request.OrderBy = Google.Apis.Calendar.v3.EventsResource.ListRequest.OrderByEnum.StartTime;

        var events = await request.ExecuteAsync(cancellationToken);

        var busySlots = new List<BusyTimeSlot>();
        if (events.Items != null)
        {
            foreach (var evt in events.Items)
            {
                // Skip transparent (free) events
                if (evt.Transparency == "transparent")
                    continue;

                var startTime = evt.Start?.DateTimeDateTimeOffset?.DateTime ?? DateTime.MinValue;
                var endTime = evt.End?.DateTimeDateTimeOffset?.DateTime ?? DateTime.MinValue;

                if (startTime != DateTime.MinValue && endTime != DateTime.MinValue)
                {
                    busySlots.Add(new BusyTimeSlot
                    {
                        StartTime = startTime,
                        EndTime = endTime
                    });
                }
            }
        }

        return busySlots;
    }

    public async Task<string> CreateCalendarEventAsync(string accessToken, CalendarEventDetails eventDetails, CancellationToken cancellationToken = default)
    {
        var service = CreateCalendarService(accessToken);

        var newEvent = new Event
        {
            Summary = eventDetails.Summary,
            Description = eventDetails.Description,
            Location = eventDetails.Location,
            Start = new EventDateTime
            {
                DateTimeDateTimeOffset = new DateTimeOffset(eventDetails.StartTime),
                TimeZone = eventDetails.TimeZone
            },
            End = new EventDateTime
            {
                DateTimeDateTimeOffset = new DateTimeOffset(eventDetails.EndTime),
                TimeZone = eventDetails.TimeZone
            }
        };

        if (eventDetails.AttendeeEmails?.Any() == true)
        {
            newEvent.Attendees = eventDetails.AttendeeEmails
                .Select(email => new EventAttendee { Email = email })
                .ToList();
        }

        if (!string.IsNullOrEmpty(eventDetails.MeetingLink))
        {
            newEvent.ConferenceData = new ConferenceData
            {
                EntryPoints = new List<EntryPoint>
                {
                    new EntryPoint
                    {
                        EntryPointType = "video",
                        Uri = eventDetails.MeetingLink
                    }
                }
            };
        }

        var insertRequest = service.Events.Insert(newEvent, "primary");
        insertRequest.SendUpdates = Google.Apis.Calendar.v3.EventsResource.InsertRequest.SendUpdatesEnum.All;

        var createdEvent = await insertRequest.ExecuteAsync(cancellationToken);
        return createdEvent.Id;
    }

    public async Task UpdateCalendarEventAsync(string accessToken, string eventId, CalendarEventDetails eventDetails, CancellationToken cancellationToken = default)
    {
        var service = CreateCalendarService(accessToken);

        // Get existing event
        var existingEvent = await service.Events.Get("primary", eventId).ExecuteAsync(cancellationToken);

        // Update fields
        existingEvent.Summary = eventDetails.Summary;
        existingEvent.Description = eventDetails.Description;
        existingEvent.Location = eventDetails.Location;
        existingEvent.Start = new EventDateTime
        {
            DateTimeDateTimeOffset = new DateTimeOffset(eventDetails.StartTime),
            TimeZone = eventDetails.TimeZone
        };
        existingEvent.End = new EventDateTime
        {
            DateTimeDateTimeOffset = new DateTimeOffset(eventDetails.EndTime),
            TimeZone = eventDetails.TimeZone
        };

        if (eventDetails.AttendeeEmails != null)
        {
            existingEvent.Attendees = eventDetails.AttendeeEmails
                .Select(email => new EventAttendee { Email = email })
                .ToList();
        }

        var updateRequest = service.Events.Update(existingEvent, "primary", eventId);
        updateRequest.SendUpdates = Google.Apis.Calendar.v3.EventsResource.UpdateRequest.SendUpdatesEnum.All;

        await updateRequest.ExecuteAsync(cancellationToken);
    }

    public async Task DeleteCalendarEventAsync(string accessToken, string eventId, CancellationToken cancellationToken = default)
    {
        var service = CreateCalendarService(accessToken);
        await service.Events.Delete("primary", eventId).ExecuteAsync(cancellationToken);
    }

    private GoogleCalendarService CreateCalendarService(string accessToken)
    {
        var credential = GoogleCredential.FromAccessToken(accessToken);
        return new GoogleCalendarService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential
        });
    }

    private GoogleAuthorizationCodeFlow CreateGoogleAuthFlow()
    {
        var clientId = _configuration["Google:ClientId"] ?? "YOUR_CLIENT_ID";
        var clientSecret = _configuration["Google:ClientSecret"] ?? "YOUR_CLIENT_SECRET";

        return new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            },
            Scopes = new[]
            {
                _externalApisSettings.Google.CalendarScope,
                _externalApisSettings.Google.EmailScope,
                _externalApisSettings.Google.ProfileScope,
                OPENID_SCOPE
            }
        });
    }
}
