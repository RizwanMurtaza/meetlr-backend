using Meetlr.Application.Interfaces.Models;

namespace Meetlr.Application.Interfaces;

/// <summary>
/// Service for OAuth operations (Google, Microsoft)
/// </summary>
public interface IOAuthService
{
    Task<OAuthUserInfo> GetUserInfoFromTokenAsync(string accessToken, string provider);
}

