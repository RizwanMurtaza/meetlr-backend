using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Meetlr.Domain.Entities.Tenancy;
using Meetlr.Domain.Entities.Users;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Meetlr.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Meetlr.Infrastructure.Services;

public class JwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;

    public JwtTokenService(IConfiguration configuration, IUnitOfWork unitOfWork)
    {
        _configuration = configuration;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> GenerateTokenAsync(User user, bool? isAdmin = null, CancellationToken cancellationToken = default)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw ConfigurationErrors.JwtSecretNotConfigured();
        var issuer = jwtSettings["Issuer"] ?? "CalendlyClone";
        var audience = jwtSettings["Audience"] ?? "CalendlyCloneUsers";
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Use provided isAdmin value, or query DB if not provided
        var adminStatus = isAdmin ?? await _unitOfWork.Repository<UserGroup>()
            .GetQueryable()
            .AnyAsync(ug => ug.UserId == user.Id && ug.Group.IsAdminGroup, cancellationToken);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("MeetlrUserId", user.Id.ToString()), // Custom claim for endpoints
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("TenantId", user.TenantId.ToString()),
            new Claim("IsAdmin", adminStatus.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public DateTime GetTokenExpiry()
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");
        return DateTime.UtcNow.AddMinutes(expiryMinutes);
    }

    /// <summary>
    /// Generates a cryptographically secure refresh token and stores it in the database.
    /// </summary>
    public async Task<RefreshToken> GenerateRefreshTokenAsync(
        User user,
        string? ipAddress = null,
        string? deviceInfo = null,
        CancellationToken cancellationToken = default)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var refreshTokenExpiryDays = int.Parse(jwtSettings["RefreshTokenExpiryDays"] ?? "7");

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TenantId = user.TenantId,
            Token = GenerateSecureToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
            IpAddress = ipAddress,
            DeviceInfo = deviceInfo,
            CreatedAt = DateTime.UtcNow
        };

        _unitOfWork.Repository<RefreshToken>().Add(refreshToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return refreshToken;
    }

    /// <summary>
    /// Validates a refresh token and returns the associated user if valid.
    /// </summary>
    public async Task<(User? User, RefreshToken? RefreshToken)> ValidateRefreshTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = await _unitOfWork.Repository<RefreshToken>()
            .GetQueryable()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

        if (refreshToken == null)
            return (null, null);

        if (!refreshToken.IsActive)
            return (null, refreshToken);

        return (refreshToken.User, refreshToken);
    }

    /// <summary>
    /// Rotates a refresh token - revokes the old one and issues a new one.
    /// </summary>
    public async Task<RefreshToken> RotateRefreshTokenAsync(
        RefreshToken oldToken,
        string? ipAddress = null,
        string? deviceInfo = null,
        CancellationToken cancellationToken = default)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var refreshTokenExpiryDays = int.Parse(jwtSettings["RefreshTokenExpiryDays"] ?? "7");

        // Create new refresh token
        var newToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = oldToken.UserId,
            TenantId = oldToken.TenantId,
            Token = GenerateSecureToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
            IpAddress = ipAddress,
            DeviceInfo = deviceInfo,
            CreatedAt = DateTime.UtcNow
        };

        // Revoke old token
        oldToken.IsRevoked = true;
        oldToken.RevokedAt = DateTime.UtcNow;
        oldToken.ReplacedByToken = newToken.Token;

        _unitOfWork.Repository<RefreshToken>().Update(oldToken);
        _unitOfWork.Repository<RefreshToken>().Add(newToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return newToken;
    }

    /// <summary>
    /// Revokes a specific refresh token.
    /// </summary>
    public async Task RevokeRefreshTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = await _unitOfWork.Repository<RefreshToken>()
            .GetQueryable()
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

        if (refreshToken != null && refreshToken.IsActive)
        {
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            _unitOfWork.Repository<RefreshToken>().Update(refreshToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Revokes all refresh tokens for a user (logout from all devices).
    /// </summary>
    public async Task RevokeAllUserRefreshTokensAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var activeTokens = await _unitOfWork.Repository<RefreshToken>()
            .GetQueryable()
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            _unitOfWork.Repository<RefreshToken>().Update(token);
        }

        if (activeTokens.Any())
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public DateTime GetRefreshTokenExpiry()
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var refreshTokenExpiryDays = int.Parse(jwtSettings["RefreshTokenExpiryDays"] ?? "7");
        return DateTime.UtcNow.AddDays(refreshTokenExpiryDays);
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
