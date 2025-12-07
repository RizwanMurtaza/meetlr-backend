using System.Net.Http.Headers;
using System.Text.Json;
using Meetlr.Application.Common.Settings;
using Meetlr.Application.Interfaces;
using Meetlr.Application.Interfaces.Models;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.Extensions.Options;

namespace Meetlr.Infrastructure.Services;

public class OAuthService : IOAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ExternalApisSettings _externalApisSettings;

    public OAuthService(HttpClient httpClient, IOptions<ExternalApisSettings> externalApisSettings)
    {
        _httpClient = httpClient;
        _externalApisSettings = externalApisSettings.Value;
    }

    public async Task<OAuthUserInfo> GetUserInfoFromTokenAsync(string accessToken, string provider)
    {
        return provider.ToLower() switch
        {
            "google" => await GetGoogleUserInfoAsync(accessToken),
            "microsoft" => await GetMicrosoftUserInfoAsync(accessToken),
            _ => throw OAuthErrors.ProviderNotSupported(provider)
        };
    }

    private async Task<OAuthUserInfo> GetGoogleUserInfoAsync(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.GetAsync(_externalApisSettings.Google.UserInfoEndpoint);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (userInfo == null)
        {
            throw OAuthErrors.UserInfoParseFailed("Google");
        }

        // Parse name into first and last name
        var nameParts = (userInfo.Name ?? "").Split(' ', 2);
        var firstName = nameParts.Length > 0 ? nameParts[0] : "";
        var lastName = nameParts.Length > 1 ? nameParts[1] : "";

        // If name is empty, use email prefix
        if (string.IsNullOrEmpty(firstName))
        {
            var emailParts = userInfo.Email.Split('@');
            firstName = emailParts[0];
        }

        return new OAuthUserInfo
        {
            Email = userInfo.Email,
            FirstName = firstName,
            LastName = lastName,
            ProfileImageUrl = userInfo.Picture,
            ProviderId = userInfo.Id,
            Provider = "Google"
        };
    }

    private async Task<OAuthUserInfo> GetMicrosoftUserInfoAsync(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.GetAsync(_externalApisSettings.Microsoft.UserInfoEndpoint);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var userInfo = JsonSerializer.Deserialize<MicrosoftUserInfo>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (userInfo == null)
        {
            throw OAuthErrors.UserInfoParseFailed("Microsoft");
        }

        return new OAuthUserInfo
        {
            Email = userInfo.Mail ?? userInfo.UserPrincipalName ?? "",
            FirstName = userInfo.GivenName ?? "",
            LastName = userInfo.Surname ?? "",
            ProfileImageUrl = null, // Microsoft Graph requires separate call for photo
            ProviderId = userInfo.Id,
            Provider = "Microsoft"
        };
    }

    // Google user info response model
    private class GoogleUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Verified_Email { get; set; }
        public string? Name { get; set; }
        public string? Given_Name { get; set; }
        public string? Family_Name { get; set; }
        public string? Picture { get; set; }
        public string? Locale { get; set; }
    }

    // Microsoft user info response model
    private class MicrosoftUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? GivenName { get; set; }
        public string? Surname { get; set; }
        public string? Mail { get; set; }
        public string? UserPrincipalName { get; set; }
        public string? JobTitle { get; set; }
        public string? MobilePhone { get; set; }
    }
}
