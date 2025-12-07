using System.Security.Cryptography;
using  Meetlr.Domain.Entities.Users;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Infrastructure.Services;

/// <summary>
/// Service for generating and validating one-time passwords (OTPs)
/// OTPs expire after 5 minutes and allow maximum 3 validation attempts
/// </summary>
public class OtpService : IOtpService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OtpService> _logger;

    private const int OTP_VALIDITY_MINUTES = 5;
    private const int MAX_VALIDATION_ATTEMPTS = 3;
    private const int CLEANUP_HOURS = 24; // Cleanup OTPs older than 24 hours

    public OtpService(
        IUnitOfWork unitOfWork,
        ILogger<OtpService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<string> GenerateOtpAsync(
        Guid userId,
        OtpPurpose purpose,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating OTP for User: {UserId}, Purpose: {Purpose}", userId, purpose);

        // Invalidate any existing unused OTPs for this user and purpose
        await InvalidateOtpsAsync(userId, purpose, cancellationToken);

        // Generate secure 6-digit code
        var code = GenerateSecureCode();

        var otp = new OtpVerification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Code = code,
            Purpose = purpose,
            ExpiresAt = DateTime.UtcNow.AddMinutes(OTP_VALIDITY_MINUTES),
            IsUsed = false,
            AttemptCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _unitOfWork.Repository<OtpVerification>().Add(otp);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("OTP generated successfully for User: {UserId}, Expires at: {ExpiresAt}",
            userId, otp.ExpiresAt);

        return code;
    }

    public async Task<bool> ValidateOtpAsync(
        Guid userId,
        string code,
        OtpPurpose purpose,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Validating OTP for User: {UserId}, Purpose: {Purpose}", userId, purpose);

        var otp = await _unitOfWork.Repository<OtpVerification>()
            .GetQueryable()
            .Where(o =>
                o.UserId == userId &&
                o.Code == code &&
                o.Purpose == purpose &&
                !o.IsUsed &&
                !o.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (otp == null)
        {
            _logger.LogWarning("OTP not found or already used for User: {UserId}, Purpose: {Purpose}", userId, purpose);
            return false;
        }

        // Track attempt
        otp.AttemptCount++;
        otp.LastAttemptAt = DateTime.UtcNow;

        // Check if expired
        if (DateTime.UtcNow > otp.ExpiresAt)
        {
            _logger.LogWarning("OTP expired for User: {UserId}, ExpiresAt: {ExpiresAt}", userId, otp.ExpiresAt);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return false;
        }

        // Check max attempts
        if (otp.AttemptCount > MAX_VALIDATION_ATTEMPTS)
        {
            _logger.LogWarning("Max validation attempts exceeded for User: {UserId}, Attempts: {Attempts}",
                userId, otp.AttemptCount);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw ValidationException.InvalidInput("OTP",
                $"Maximum verification attempts ({MAX_VALIDATION_ATTEMPTS}) exceeded. Please request a new verification code.");
        }

        // Valid OTP - mark as used
        otp.IsUsed = true;
        otp.UsedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("OTP validated successfully for User: {UserId}, Purpose: {Purpose}", userId, purpose);

        return true;
    }

    public async Task InvalidateOtpsAsync(
        Guid userId,
        OtpPurpose purpose,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Invalidating OTPs for User: {UserId}, Purpose: {Purpose}", userId, purpose);

        var existingOtps = await _unitOfWork.Repository<OtpVerification>()
            .GetQueryable()
            .Where(o =>
                o.UserId == userId &&
                o.Purpose == purpose &&
                !o.IsUsed &&
                !o.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!existingOtps.Any())
        {
            _logger.LogDebug("No active OTPs found to invalidate");
            return;
        }

        foreach (var otp in existingOtps)
        {
            otp.IsDeleted = true;
            otp.DeletedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Invalidated {Count} OTP(s) for User: {UserId}, Purpose: {Purpose}",
            existingOtps.Count, userId, purpose);
    }

    public async Task CleanupExpiredOtpsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting OTP cleanup (removing OTPs older than {Hours} hours)", CLEANUP_HOURS);

        var cutoffDate = DateTime.UtcNow.AddHours(-CLEANUP_HOURS);

        var expiredOtps = await _unitOfWork.Repository<OtpVerification>()
            .GetQueryable()
            .Where(o => o.CreatedAt < cutoffDate && !o.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!expiredOtps.Any())
        {
            _logger.LogInformation("No expired OTPs found for cleanup");
            return;
        }

        foreach (var otp in expiredOtps)
        {
            otp.IsDeleted = true;
            otp.DeletedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cleaned up {Count} expired OTP(s)", expiredOtps.Count);
    }

    /// <summary>
    /// Generate a cryptographically secure 6-digit code
    /// </summary>
    private string GenerateSecureCode()
    {
        // Generate a secure random number between 000000 and 999999
        var bytes = new byte[4];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);

        var number = Math.Abs(BitConverter.ToInt32(bytes, 0));
        var code = (number % 1000000).ToString("D6");

        return code;
    }
}
