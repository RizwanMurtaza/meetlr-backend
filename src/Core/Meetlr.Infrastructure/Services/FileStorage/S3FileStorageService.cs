using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Meetlr.Infrastructure.Services.FileStorage;

public class S3FileStorageService : IFileStorageService, IDisposable
{
    private readonly FileStorageSettings _settings;
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3FileStorageService> _logger;
    private readonly string _bucketName;
    private readonly string? _cdnUrl;

    public string ProviderName => "S3";

    public S3FileStorageService(
        IOptions<FileStorageSettings> settings,
        ILogger<S3FileStorageService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        var s3Settings = _settings.S3;
        _bucketName = s3Settings.BucketName ?? throw ConfigurationErrors.S3BucketNotConfigured();
        _cdnUrl = s3Settings.CdnUrl?.TrimEnd('/');

        // Configure S3 client
        var config = new AmazonS3Config
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(s3Settings.Region),
            ForcePathStyle = s3Settings.ForcePathStyle
        };

        // Support for S3-compatible services (MinIO, DigitalOcean Spaces, etc.)
        if (!string.IsNullOrEmpty(s3Settings.ServiceUrl))
        {
            config.ServiceURL = s3Settings.ServiceUrl;
        }

        _s3Client = new AmazonS3Client(
            s3Settings.AccessKeyId,
            s3Settings.SecretAccessKey,
            config);
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
            // Validate file size
            if (fileStream.Length > _settings.MaxFileSizeBytes)
            {
                return FileUploadResult.Failed($"File size exceeds maximum allowed size of {_settings.MaxFileSizeBytes / 1024 / 1024}MB");
            }

            // Validate extension
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (_settings.AllowedExtensions.Length > 0 && !_settings.AllowedExtensions.Contains(extension))
            {
                return FileUploadResult.Failed($"File extension '{extension}' is not allowed");
            }

            // Generate unique key
            var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            var key = $"{folder}/{uniqueFileName}";

            // Upload using TransferUtility for better handling of large files
            using var transferUtility = new TransferUtility(_s3Client);
            var uploadRequest = new TransferUtilityUploadRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = fileStream,
                ContentType = contentType,
                CannedACL = S3CannedACL.PublicRead // Make publicly readable
            };

            // Add cache control header for better performance
            uploadRequest.Headers.CacheControl = "public, max-age=31536000"; // 1 year

            await transferUtility.UploadAsync(uploadRequest, cancellationToken);

            // Generate URL
            string url;
            if (!string.IsNullOrEmpty(_cdnUrl))
            {
                url = $"{_cdnUrl}/{key}";
            }
            else
            {
                url = $"https://{_bucketName}.s3.{_settings.S3.Region}.amazonaws.com/{key}";
            }

            _logger.LogInformation("File uploaded to S3: {Key}", key);

            return FileUploadResult.Succeeded(url, key, fileStream.Length, contentType);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "S3 error uploading file: {FileName}", fileName);
            return FileUploadResult.Failed($"S3 error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file to S3: {FileName}", fileName);
            return FileUploadResult.Failed($"Failed to upload file: {ex.Message}");
        }
    }

    public async Task<bool> DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            // Extract key from URL
            var key = ExtractKeyFromUrl(fileUrl);
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(deleteRequest, cancellationToken);
            _logger.LogInformation("File deleted from S3: {Key}", key);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file from S3: {FileUrl}", fileUrl);
            return false;
        }
    }

    public async Task<bool> ExistsAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = ExtractKeyFromUrl(fileUrl);
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            var request = new GetObjectMetadataRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.GetObjectMetadataAsync(request, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }

    private string? ExtractKeyFromUrl(string url)
    {
        // Handle CDN URLs
        if (!string.IsNullOrEmpty(_cdnUrl) && url.StartsWith(_cdnUrl))
        {
            return url.Substring(_cdnUrl.Length).TrimStart('/');
        }

        // Handle S3 URLs
        var s3UrlPrefix = $"https://{_bucketName}.s3.{_settings.S3.Region}.amazonaws.com/";
        if (url.StartsWith(s3UrlPrefix))
        {
            return url.Substring(s3UrlPrefix.Length);
        }

        // Try to extract from path-style URL
        var pathStylePrefix = $"https://s3.{_settings.S3.Region}.amazonaws.com/{_bucketName}/";
        if (url.StartsWith(pathStylePrefix))
        {
            return url.Substring(pathStylePrefix.Length);
        }

        return null;
    }

    public void Dispose()
    {
        _s3Client?.Dispose();
    }
}
