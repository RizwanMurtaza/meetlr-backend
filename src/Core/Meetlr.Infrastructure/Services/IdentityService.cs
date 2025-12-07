using  Meetlr.Domain.Entities.Users;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.AspNetCore.Identity;

namespace Meetlr.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<User> _userManager;
    private readonly JwtTokenService _jwtTokenService;

    public IdentityService(
        UserManager<User> userManager,
        JwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<(bool Succeeded, string[] Errors, Guid UserId)> CreateUserAsync(string email, string password)
    {
        var applicationUser = new User
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            TenantId = Guid.Empty, // TODO: Update this when implementing proper multi-tenancy for existing users
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(applicationUser, password);

        if (result.Succeeded)
        {
            return (true, Array.Empty<string>(), applicationUser.Id);
        }

        var errors = result.Errors.Select(e => e.Description).ToArray();
        return (false, errors, Guid.Empty);
    }

    public async Task<(bool Succeeded, string[] Errors, Guid UserId)> CreateUserWithTenantAsync(
        string email,
        string password,
        Guid tenantId,
        Guid userId)
    {
        var applicationUser = new User
        {
            Id = userId,
            UserName = email,
            Email = email,
            // User.Id is the primary identifier
            TenantId = tenantId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(applicationUser, password);

        if (result.Succeeded)
        {
            return (true, Array.Empty<string>(), applicationUser.Id);
        }

        var errors = result.Errors.Select(e => e.Description).ToArray();
        return (false, errors, Guid.Empty);
    }

    public async Task<(bool Succeeded, string[] Errors, Guid UserId)> CreateOAuthUserAsync(
        string email,
        string provider,
        string providerId,
        Guid tenantId,
        Guid userId)
    {
        var applicationUser = new User
        {
            Id = userId,
            UserName = email,
            Email = email,
            EmailConfirmed = true, // OAuth users have verified emails
            // User.Id is the primary identifier
            TenantId = tenantId,
            OAuthProvider = provider,
            OAuthProviderId = providerId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Create without password since OAuth users don't use passwords
        var result = await _userManager.CreateAsync(applicationUser);

        if (result.Succeeded)
        {
            return (true, Array.Empty<string>(), applicationUser.Id);
        }

        var errors = result.Errors.Select(e => e.Description).ToArray();
        return (false, errors, Guid.Empty);
    }

    public async Task<(bool Succeeded, Guid UserId)> ValidateOAuthUserAsync(string email, string provider, string providerId)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return (false, Guid.Empty);
        }

        if (!user.IsActive)
        {
            return (false, Guid.Empty);
        }

        // Verify OAuth provider matches
        if (user.OAuthProvider != provider || user.OAuthProviderId != providerId)
        {
            return (false, Guid.Empty);
        }

        return (true, user.Id);
    }

    public async Task<(bool Succeeded, Guid UserId)> ValidateCredentialsAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return (false, Guid.Empty);
        }

        if (!user.IsActive)
        {
            return (false, Guid.Empty);
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!passwordValid)
        {
            return (false, Guid.Empty);
        }

        return (true, user.Id);
    }

    public async Task<string> GenerateTokenAsync(Guid userId, string email, bool? isAdmin = null, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw UserErrors.UserNotFound(userId);
        }

        return await _jwtTokenService.GenerateTokenAsync(user, isAdmin, cancellationToken);
    }

    public Task<DateTime> GetTokenExpiryAsync()
    {
        return Task.FromResult(_jwtTokenService.GetTokenExpiry());
    }

    public async Task UpdateLastLoginAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
        }
    }

    public async Task<string> GenerateRefreshTokenAsync(
        Guid userId,
        string? ipAddress = null,
        string? deviceInfo = null,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw UserErrors.UserNotFound(userId);
        }

        var refreshToken = await _jwtTokenService.GenerateRefreshTokenAsync(user, ipAddress, deviceInfo, cancellationToken);
        return refreshToken.Token;
    }

    public async Task<(bool IsValid, Guid UserId, string? NewAccessToken, string? NewRefreshToken, DateTime? AccessTokenExpiry, DateTime? RefreshTokenExpiry)> RefreshTokenAsync(
        string refreshToken,
        bool? isAdmin = null,
        string? ipAddress = null,
        string? deviceInfo = null,
        CancellationToken cancellationToken = default)
    {
        var (user, token) = await _jwtTokenService.ValidateRefreshTokenAsync(refreshToken, cancellationToken);

        if (user == null || token == null || !token.IsActive)
        {
            return (false, Guid.Empty, null, null, null, null);
        }

        // Rotate the refresh token
        var newRefreshToken = await _jwtTokenService.RotateRefreshTokenAsync(token, ipAddress, deviceInfo, cancellationToken);

        // Generate new access token
        var newAccessToken = await _jwtTokenService.GenerateTokenAsync(user, isAdmin, cancellationToken);
        var accessTokenExpiry = _jwtTokenService.GetTokenExpiry();
        var refreshTokenExpiry = _jwtTokenService.GetRefreshTokenExpiry();

        return (true, user.Id, newAccessToken, newRefreshToken.Token, accessTokenExpiry, refreshTokenExpiry);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        await _jwtTokenService.RevokeRefreshTokenAsync(refreshToken, cancellationToken);
    }

    public async Task RevokeAllUserRefreshTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await _jwtTokenService.RevokeAllUserRefreshTokensAsync(userId, cancellationToken);
    }

    public DateTime GetRefreshTokenExpiry()
    {
        return _jwtTokenService.GetRefreshTokenExpiry();
    }
}
