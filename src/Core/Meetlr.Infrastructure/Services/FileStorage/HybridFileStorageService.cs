using Meetlr.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Meetlr.Infrastructure.Services.FileStorage;

/// <summary>
/// Hybrid file storage service that automatically selects the best provider.
/// Uses S3 when configured, falls back to local storage.
/// </summary>
public class HybridFileStorageService : IFileStorageService
{
    private readonly IFileStorageService _primaryService;
    private readonly IFileStorageService? _fallbackService;
    private readonly ILogger<HybridFileStorageService> _logger;

    public string ProviderName => _primaryService.ProviderName;

    public HybridFileStorageService(
        IOptions<FileStorageSettings> settings,
        ILogger<HybridFileStorageService> logger,
        ILogger<S3FileStorageService> s3Logger,
        ILogger<LocalFileStorageService> localLogger,
        IWebHostEnvironmentAccessor webHostAccessor)
    {
        _logger = logger;
        var config = settings.Value;

        // Determine which provider to use
        var provider = config.Provider.ToLowerInvariant();

        if (provider == "s3" || (provider == "auto" && IsS3Configured(config.S3)))
        {
            try
            {
                _primaryService = new S3FileStorageService(settings, s3Logger);
                _fallbackService = new LocalFileStorageService(settings, localLogger, webHostAccessor);
                _logger.LogInformation("File storage using S3 as primary with local fallback");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to initialize S3 storage, using local storage");
                _primaryService = new LocalFileStorageService(settings, localLogger, webHostAccessor);
                _fallbackService = null;
            }
        }
        else
        {
            _primaryService = new LocalFileStorageService(settings, localLogger, webHostAccessor);
            _fallbackService = null;
            _logger.LogInformation("File storage using local storage");
        }
    }

    private static bool IsS3Configured(S3Settings s3)
    {
        return !string.IsNullOrEmpty(s3.AccessKeyId)
            && !string.IsNullOrEmpty(s3.SecretAccessKey)
            && !string.IsNullOrEmpty(s3.BucketName);
    }

    public async Task<FileUploadResult> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string folder = "uploads",
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Reset stream position for potential retry
            var canSeek = fileStream.CanSeek;
            var originalPosition = canSeek ? fileStream.Position : 0;

            var result = await _primaryService.UploadAsync(fileStream, fileName, contentType, folder, cancellationToken);

            if (result.Success)
            {
                return result;
            }

            // Try fallback if available
            if (_fallbackService != null)
            {
                _logger.LogWarning("Primary storage failed, trying fallback: {Error}", result.Error);

                if (canSeek)
                {
                    fileStream.Position = originalPosition;
                }

                return await _fallbackService.UploadAsync(fileStream, fileName, contentType, folder, cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", fileName);

            // Try fallback on exception
            if (_fallbackService != null)
            {
                try
                {
                    if (fileStream.CanSeek)
                    {
                        fileStream.Position = 0;
                    }
                    return await _fallbackService.UploadAsync(fileStream, fileName, contentType, folder, cancellationToken);
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogError(fallbackEx, "Fallback storage also failed");
                }
            }

            return FileUploadResult.Failed($"Failed to upload file: {ex.Message}");
        }
    }

    public async Task<bool> DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        // Try primary first
        if (await _primaryService.DeleteAsync(fileUrl, cancellationToken))
        {
            return true;
        }

        // Try fallback if available
        if (_fallbackService != null)
        {
            return await _fallbackService.DeleteAsync(fileUrl, cancellationToken);
        }

        return false;
    }

    public async Task<bool> ExistsAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        // Check primary first
        if (await _primaryService.ExistsAsync(fileUrl, cancellationToken))
        {
            return true;
        }

        // Check fallback if available
        if (_fallbackService != null)
        {
            return await _fallbackService.ExistsAsync(fileUrl, cancellationToken);
        }

        return false;
    }
}

/// <summary>
/// Interface to access IWebHostEnvironment from infrastructure layer
/// </summary>
public interface IWebHostEnvironmentAccessor
{
    string WebRootPath { get; }
    string ContentRootPath { get; }
}
