using System.Text.Json;
using Meetlr.Application.Common.Settings;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Meetlr.Infrastructure.Services.OAuth.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Meetlr.Infrastructure.Services;

public class MicrosoftOAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ExternalApisSettings _externalApisSettings;

    public MicrosoftOAuthService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IOptions<ExternalApisSettings> externalApisSettings)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _externalApisSettings = externalApisSettings.Value;
    }

    public string GetAuthorizationUrl(string redirectUri, string state)
    {
        var clientId = _configuration["Microsoft:ClientId"] ?? "YOUR_CLIENT_ID";
        var scopes = Uri.EscapeDataString($"{_externalApisSettings.Microsoft.CalendarScope} {_externalApisSettings.Microsoft.EmailScope} offline_access openid profile email");

        return $"{_externalApisSettings.Microsoft.Authority}/oauth2/v2.0/authorize?" +
               $"client_id={clientId}" +
               $"&response_type=code" +
               $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
               $"&response_mode=query" +
               $"&scope={scopes}" +
               $"&state={state}";
    }

    public async Task<MicrosoftTokenResponse> ExchangeCodeForTokenAsync(string code, string redirectUri)
    {
        var clientId = _configuration["Microsoft:ClientId"] ?? "YOUR_CLIENT_ID";
        var clientSecret = _configuration["Microsoft:ClientSecret"] ?? "YOUR_CLIENT_SECRET";

        var httpClient = _httpClientFactory.CreateClient();
        var tokenEndpoint = $"{_externalApisSettings.Microsoft.Authority}/oauth2/v2.0/token";

        // Log for debugging
        Console.WriteLine($"[Microsoft OAuth] Code length: {code.Length}");
        Console.WriteLine($"[Microsoft OAuth] RedirectUri: {redirectUri}");

        // Use the code as-is - FormUrlEncodedContent handles encoding
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

        Console.WriteLine($"[Microsoft OAuth] Response status: {response.StatusCode}");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"[Microsoft OAuth] Error: {responseBody}");
            throw OAuthErrors.TokenExchangeFailed("Microsoft", $"Failed to exchange code for token: {responseBody}");
        }

        var tokenResponse = JsonSerializer.Deserialize<MicrosoftTokenResponse>(responseBody);
        return tokenResponse ?? throw OAuthErrors.TokenExchangeFailed("Microsoft", "Failed to deserialize token response");
    }

    public async Task<MicrosoftTokenResponse> RefreshTokenAsync(string refreshToken)
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
        var response = await httpClient.PostAsync(tokenEndpoint, content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw OAuthErrors.TokenRefreshFailed("Microsoft", $"Failed to refresh token: {responseBody}");
        }

        var tokenResponse = JsonSerializer.Deserialize<MicrosoftTokenResponse>(responseBody);
        return tokenResponse ?? throw OAuthErrors.TokenRefreshFailed("Microsoft", "Failed to deserialize token response");
    }

    public async Task<MicrosoftUserInfo> GetUserInfoAsync(string accessToken)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.GetAsync("https://graph.microsoft.com/v1.0/me");
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw OAuthErrors.TokenExchangeFailed("Microsoft", $"Failed to get user info: {responseBody}");
        }

        var userInfo = JsonSerializer.Deserialize<MicrosoftUserInfo>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return userInfo ?? throw OAuthErrors.TokenExchangeFailed("Microsoft", "Failed to deserialize user info");
    }
}

public class MicrosoftUserInfo
{
    public string Id { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Mail { get; set; }
    public string? UserPrincipalName { get; set; }

    // Email is either Mail or UserPrincipalName (UPN)
    public string Email => Mail ?? UserPrincipalName ?? string.Empty;
}

