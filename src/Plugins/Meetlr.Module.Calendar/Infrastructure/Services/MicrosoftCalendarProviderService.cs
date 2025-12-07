using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Meetlr.Application.Common.Settings;
using Meetlr.Module.Calendar.Application.Interfaces;
using Meetlr.Module.Calendar.Application.Models;
using Meetlr.Module.Calendar.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Meetlr.Module.Calendar.Infrastructure.Services;

/// <summary>
/// Microsoft/Outlook Calendar provider service implementing ICalendarProviderService for OAuth operations.
/// Self-contained OAuth implementation without external dependencies.
/// </summary>
internal class MicrosoftCalendarProviderService : ICalendarProviderService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ExternalApisSettings _externalApisSettings;

    public MicrosoftCalendarProviderService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IOptions<ExternalApisSettings> externalApisSettings)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _externalApisSettings = externalApisSettings.Value;
    }

    private const string GraphApiBaseUrl = "https://graph.microsoft.com/v1.0";

    public CalendarProvider Provider => CalendarProvider.Outlook;

    public string ProviderName => "Outlook Calendar";

    public string GetAuthorizationUrl(string redirectUri, string state)
    {
        var clientId = _configuration["Microsoft:ClientId"] ?? "YOUR_CLIENT_ID";
        var scopes = Uri.EscapeDataString(
            $"{_externalApisSettings.Microsoft.CalendarScope} " +
            $"{_externalApisSettings.Microsoft.EmailScope} " +
            "offline_access openid profile email");

        return $"{_externalApisSettings.Microsoft.Authority}/oauth2/v2.0/authorize?" +
               $"client_id={clientId}" +
               $"&response_type=code" +
               $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
               $"&response_mode=query" +
               $"&scope={scopes}" +
               $"&state={state}";
    }

    public async Task<TokenRefreshResult> ExchangeCodeForTokenAsync(string code, string redirectUri)
    {
        var clientId = _configuration["Microsoft:ClientId"] ?? "YOUR_CLIENT_ID";
        var clientSecret = _configuration["Microsoft:ClientSecret"] ?? "YOUR_CLIENT_SECRET";

        var httpClient = _httpClientFactory.CreateClient();
        var tokenEndpoint = $"{_externalApisSettings.Microsoft.Authority}/oauth2/v2.0/token";

        var parameters = new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "code", code },
            { "redirect_uri", redirectUri },
            { "grant_type", "authorization_code" },
            { "scope", $"{_externalApisSettings.Microsoft.CalendarScope} {_externalApisSettings.Microsoft.EmailScope} offline_access openid profile email" }
        };

        var content = new FormUrlEncodedContent(parameters);
        var response = await httpClient.PostAsync(tokenEndpoint, content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to exchange code for token: {responseBody}");
        }

        var tokenResponse = JsonSerializer.Deserialize<MicrosoftTokenResponse>(responseBody)
            ?? throw new InvalidOperationException("Failed to deserialize token response");

        return new TokenRefreshResult
        {
            AccessToken = tokenResponse.access_token,
            RefreshToken = tokenResponse.refresh_token,
            ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.expires_in)
        };
    }

    public async Task<string?> GetUserEmailAsync(string accessToken)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.GetAsync(_externalApisSettings.Microsoft.UserInfoEndpoint);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var userInfo = JsonDocument.Parse(json);

                // Try mail first, then userPrincipalName
                if (userInfo.RootElement.TryGetProperty("mail", out var mailElement) && mailElement.ValueKind != JsonValueKind.Null)
                {
                    return mailElement.GetString();
                }

                if (userInfo.RootElement.TryGetProperty("userPrincipalName", out var upnElement))
                {
                    return upnElement.GetString();
                }
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
        var clientId = _configuration["Microsoft:ClientId"] ?? "YOUR_CLIENT_ID";
        var clientSecret = _configuration["Microsoft:ClientSecret"] ?? "YOUR_CLIENT_SECRET";

        var httpClient = _httpClientFactory.CreateClient();
        var tokenEndpoint = $"{_externalApisSettings.Microsoft.Authority}/oauth2/v2.0/token";

        var parameters = new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "refresh_token", refreshToken },
            { "grant_type", "refresh_token" },
            { "scope", $"{_externalApisSettings.Microsoft.CalendarScope} {_externalApisSettings.Microsoft.EmailScope} offline_access openid profile email" }
        };

        var content = new FormUrlEncodedContent(parameters);
        var response = await httpClient.PostAsync(tokenEndpoint, content, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to refresh token: {responseBody}");
        }

        var tokenResponse = JsonSerializer.Deserialize<MicrosoftTokenResponse>(responseBody)
            ?? throw new InvalidOperationException("Failed to deserialize token response");

        return new TokenRefreshResult
        {
            AccessToken = tokenResponse.access_token,
            RefreshToken = tokenResponse.refresh_token,
            ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.expires_in)
        };
    }

    public async Task<List<BusyTimeSlot>> GetBusyTimesAsync(string accessToken, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var httpClient = CreateGraphClient(accessToken);

        var startTimeStr = startDate.ToString("o");
        var endTimeStr = endDate.ToString("o");
        var url = $"{GraphApiBaseUrl}/me/calendarView?startDateTime={startTimeStr}&endDateTime={endTimeStr}&$orderby=start/dateTime";

        var response = await httpClient.GetAsync(url, cancellationToken);

        var busySlots = new List<BusyTimeSlot>();
        if (!response.IsSuccessStatusCode)
        {
            return busySlots;
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonDocument.Parse(json);

        foreach (var evt in result.RootElement.GetProperty("value").EnumerateArray())
        {
            // Skip free events
            var showAs = evt.TryGetProperty("showAs", out var showAsElement) ? showAsElement.GetString() : "busy";
            if (showAs == "free")
                continue;

            var startElement = evt.GetProperty("start");
            var endElement = evt.GetProperty("end");

            DateTime startTime = DateTime.MinValue;
            DateTime endTime = DateTime.MinValue;

            if (startElement.TryGetProperty("dateTime", out var startDt))
            {
                DateTime.TryParse(startDt.GetString(), out startTime);
            }
            if (endElement.TryGetProperty("dateTime", out var endDt))
            {
                DateTime.TryParse(endDt.GetString(), out endTime);
            }

            if (startTime != DateTime.MinValue && endTime != DateTime.MinValue)
            {
                busySlots.Add(new BusyTimeSlot
                {
                    StartTime = startTime,
                    EndTime = endTime
                });
            }
        }

        return busySlots;
    }

    public async Task<string> CreateCalendarEventAsync(string accessToken, CalendarEventDetails eventDetails, CancellationToken cancellationToken = default)
    {
        var httpClient = CreateGraphClient(accessToken);

        var eventPayload = new
        {
            subject = eventDetails.Summary,
            body = new
            {
                contentType = "Text",
                content = eventDetails.Description ?? ""
            },
            start = new
            {
                dateTime = eventDetails.StartTime.ToString("o"),
                timeZone = eventDetails.TimeZone
            },
            end = new
            {
                dateTime = eventDetails.EndTime.ToString("o"),
                timeZone = eventDetails.TimeZone
            },
            location = new
            {
                displayName = eventDetails.Location ?? ""
            },
            attendees = eventDetails.AttendeeEmails?.Select(email => new
            {
                emailAddress = new { address = email },
                type = "required"
            }).ToArray() ?? Array.Empty<object>()
        };

        var content = new StringContent(
            JsonSerializer.Serialize(eventPayload),
            Encoding.UTF8,
            "application/json");

        var response = await httpClient.PostAsync($"{GraphApiBaseUrl}/me/events", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Failed to create event: {error}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonDocument.Parse(jsonResponse);
        return result.RootElement.GetProperty("id").GetString() ?? "";
    }

    public async Task UpdateCalendarEventAsync(string accessToken, string eventId, CalendarEventDetails eventDetails, CancellationToken cancellationToken = default)
    {
        var httpClient = CreateGraphClient(accessToken);

        var updatePayload = new
        {
            subject = eventDetails.Summary,
            body = new
            {
                contentType = "Text",
                content = eventDetails.Description ?? ""
            },
            start = new
            {
                dateTime = eventDetails.StartTime.ToString("o"),
                timeZone = eventDetails.TimeZone ?? "UTC"
            },
            end = new
            {
                dateTime = eventDetails.EndTime.ToString("o"),
                timeZone = eventDetails.TimeZone ?? "UTC"
            },
            location = new
            {
                displayName = eventDetails.Location ?? ""
            },
            attendees = eventDetails.AttendeeEmails?.Select(email => new
            {
                emailAddress = new { address = email },
                type = "required"
            }).ToArray() ?? Array.Empty<object>()
        };

        var content = new StringContent(
            JsonSerializer.Serialize(updatePayload),
            Encoding.UTF8,
            "application/json");

        var response = await httpClient.PatchAsync($"{GraphApiBaseUrl}/me/events/{eventId}", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Failed to update event: {error}");
        }
    }

    public async Task DeleteCalendarEventAsync(string accessToken, string eventId, CancellationToken cancellationToken = default)
    {
        var httpClient = CreateGraphClient(accessToken);
        var response = await httpClient.DeleteAsync($"{GraphApiBaseUrl}/me/events/{eventId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Failed to delete event: {error}");
        }
    }

    private HttpClient CreateGraphClient(string accessToken)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return httpClient;
    }

    /// <summary>
    /// Internal model for Microsoft OAuth token response
    /// </summary>
    private class MicrosoftTokenResponse
    {
        public string access_token { get; set; } = string.Empty;
        public string token_type { get; set; } = string.Empty;
        public int expires_in { get; set; }
        public string? refresh_token { get; set; }
        public string scope { get; set; } = string.Empty;
    }
}
