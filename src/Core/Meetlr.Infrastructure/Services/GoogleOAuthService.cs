using System.Net.Http.Headers;
using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Meetlr.Application.Common.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Meetlr.Infrastructure.Services;

public class GoogleOAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ExternalApisSettings _externalApisSettings;
    private readonly IHttpClientFactory _httpClientFactory;
    private const string OPENID_SCOPE = "openid";

    public GoogleOAuthService(
        IConfiguration configuration,
        IOptions<ExternalApisSettings> externalApisSettings,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _externalApisSettings = externalApisSettings.Value;
        _httpClientFactory = httpClientFactory;
    }

    public string GetAuthorizationUrl(string redirectUri, string state)
    {
        var clientId = _configuration["Google:ClientId"] ?? "YOUR_CLIENT_ID";
        var clientSecret = _configuration["Google:ClientSecret"] ?? "YOUR_CLIENT_SECRET";

        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            },
            Scopes = new[] { _externalApisSettings.Google.CalendarScope, _externalApisSettings.Google.EmailScope, _externalApisSettings.Google.ProfileScope, OPENID_SCOPE }
        });

        var authorizationUrl = flow.CreateAuthorizationCodeRequest(redirectUri);
        authorizationUrl.State = state;

        return authorizationUrl.Build().ToString();
    }

    public async Task<TokenResponse> ExchangeCodeForTokenAsync(string code, string redirectUri)
    {
        var clientId = _configuration["Google:ClientId"] ?? "YOUR_CLIENT_ID";
        var clientSecret = _configuration["Google:ClientSecret"] ?? "YOUR_CLIENT_SECRET";

        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            },
            Scopes = new[] { _externalApisSettings.Google.CalendarScope, _externalApisSettings.Google.EmailScope, _externalApisSettings.Google.ProfileScope, OPENID_SCOPE }
        });

        var token = await flow.ExchangeCodeForTokenAsync(
            userId: "user",
            code: code,
            redirectUri: redirectUri,
            CancellationToken.None);

        return token;
    }

    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
    {
        var clientId = _configuration["Google:ClientId"] ?? "YOUR_CLIENT_ID";
        var clientSecret = _configuration["Google:ClientSecret"] ?? "YOUR_CLIENT_SECRET";

        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            },
            Scopes = new[] { _externalApisSettings.Google.CalendarScope, _externalApisSettings.Google.EmailScope, _externalApisSettings.Google.ProfileScope, OPENID_SCOPE }
        });

        var token = await flow.RefreshTokenAsync(
            userId: "user",
            refreshToken: refreshToken,
            CancellationToken.None);

        return token;
    }

    public async Task<GoogleUserInfo> GetUserInfoAsync(string accessToken)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return userInfo ?? throw new InvalidOperationException("Failed to get Google user info");
    }
}

public class GoogleUserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? GivenName { get; set; }
    public string? FamilyName { get; set; }
    public string? Picture { get; set; }
}
