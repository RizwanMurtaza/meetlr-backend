namespace Meetlr.Application.Interfaces;

/// <summary>
/// Abstraction for ASP.NET Core Identity operations
/// </summary>
public interface IIdentityService
{
    Task<(bool Succeeded, string[] Errors, Guid UserId)> CreateUserAsync(string email, string password);
    Task<(bool Succeeded, string[] Errors, Guid UserId)> CreateUserWithTenantAsync(string email, string password, Guid tenantId, Guid userId);
    Task<(bool Succeeded, string[] Errors, Guid UserId)> CreateOAuthUserAsync(string email, string provider, string providerId, Guid tenantId, Guid userId);
    Task<(bool Succeeded, Guid UserId)> ValidateCredentialsAsync(string email, string password);
    Task<(bool Succeeded, Guid UserId)> ValidateOAuthUserAsync(string email, string provider, string providerId);
    Task<string> GenerateTokenAsync(Guid userId, string email, bool? isAdmin = null, CancellationToken cancellationToken = default);
    Task<DateTime> GetTokenExpiryAsync();
    Task UpdateLastLoginAsync(Guid userId);

    // Refresh token operations
    Task<string> GenerateRefreshTokenAsync(Guid userId, string? ipAddress = null, string? deviceInfo = null, CancellationToken cancellationToken = default);
    Task<(bool IsValid, Guid UserId, string? NewAccessToken, string? NewRefreshToken, DateTime? AccessTokenExpiry, DateTime? RefreshTokenExpiry)> RefreshTokenAsync(string refreshToken, bool? isAdmin = null, string? ipAddress = null, string? deviceInfo = null, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task RevokeAllUserRefreshTokensAsync(Guid userId, CancellationToken cancellationToken = default);
    DateTime GetRefreshTokenExpiry();
}
